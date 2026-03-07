using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Models;
using MUMbackend.Dtos.Comments;
using MUMbackend.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace MUMbackend.Services
{
    public class CommentService
    {
        private readonly AppDbContext _db;
        private readonly NotificationService _notificationService;

        public CommentService(AppDbContext db, NotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        public async Task<CommentResponseDto> CreateAsync(CreateCommentDto dto)
        {

            var cmt = new Comment
            {
                UserId = dto.UserId,
                SongId = dto.SongId,
                Content = dto.Content,
                ParentId = dto.ParentId
            };

            _db.Comments.Add(cmt);
            await _db.SaveChangesAsync();

            int? targetUserId = null;
            string message = "";

            // 🔔 Nếu là reply comment
            if (dto.ParentId != null)
            {
                var parentComment = await _db.Comments
                    .Where(c => c.Id == dto.ParentId)
                    .Select(c => new { c.UserId })
                    .FirstOrDefaultAsync();

                if (parentComment != null)
                {
                    targetUserId = parentComment.UserId;
                    message = $" đã trả lời bình luận của bạn";
                }
            }
            else
            {
                // 🔔 Comment vào bài hát
                var song = await _db.Songs
                    .Where(s => s.Id == dto.SongId)
                    .Select(s => new { s.ArtistId, s.Title })
                    .FirstOrDefaultAsync();

                if (song != null)
                {
                    targetUserId = song.ArtistId;
                    message = $" đã bình luận bài hát \"{song.Title}\" của bạn";
                }
            }

            // 🔔 Trigger notification
            if (targetUserId != null && targetUserId != dto.UserId)
            {
                await _notificationService.CreateNotification(
                    targetUserId.Value,
                    dto.UserId,
                    message
                );
            }

            return cmt.ToCommentDto();
        }

        public async Task<List<CommentResponseDto>> GetBySongIdAsync(int songId)
        {
            var comments = await _db.Comments
                .Where(c => c.SongId == songId && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(c => c.ToCommentDto()).ToList();
        }

        private List<CommentResponseDto> BuildCommentTree(List<CommentResponseDto> flat)
        {
            // Lookup comment theo Id
            var lookup = flat.ToDictionary(c => c.Id, c => c);

            // Root = comment không có parent
            var roots = flat.Where(c => c.ParentId == null).ToList();

            // Gán tất cả comment con trong một dictionary: ParentId -> List children
            var groupByParent = flat
                .Where(c => c.ParentId != null)
                .GroupBy(c => c.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Xử lý từng root
            foreach (var root in roots)
            {
                root.Children = new List<CommentResponseDto>();

                // Lấy toàn bộ con/cháu/chắt…
                var stack = new Stack<int>();
                stack.Push(root.Id);

                while (stack.Count > 0)
                {
                    var currentId = stack.Pop();

                    if (groupByParent.TryGetValue(currentId, out var children))
                    {
                        foreach (var child in children)
                        {
                            // Đẩy vào cấp 2 của root, KHÔNG tạo thêm cấp
                            root.Children.Add(child);

                            // Tiếp tục tìm con của child
                            stack.Push(child.Id);
                        }
                    }
                }
            }

            return roots;
        }



        public async Task<PagedResult<CommentResponseDto>> GetCommentsForSong(int songId, int page, int pageSize)
        {
            // 1. Lấy FULL comment + user (1 JOIN duy nhất)
            var flat = await _db.Comments
                .Where(c => c.SongId == songId && !c.IsDeleted)
                .Join(
                    _db.Users,
                    c => c.UserId,
                    u => u.Id,
                    (c, u) => new
                    {
                        Comment = c,
                        UserName = u.Username,
                        Avatar = u.AvatarUrl
                    }
                )
                .OrderBy(x => x.Comment.CreatedAt)
                .ToListAsync();

            // 2. Tạo dictionary: CommentId → UserName
            var userByCommentId = flat.ToDictionary(
                x => x.Comment.Id,
                x => x.UserName
            );

            // 3. Map sang DTO + gắn ParentUserName
            var dtoList = flat.Select(x => new CommentResponseDto
            {
                Id = x.Comment.Id,
                SongId = x.Comment.SongId,
                ParentId = x.Comment.ParentId,
                Content = x.Comment.Content,
                CreatedAt = x.Comment.CreatedAt,
                UpdatedAt = x.Comment.UpdatedAt,
                IsDeleted = x.Comment.IsDeleted,
                UserId = x.Comment.UserId,
                UserName = x.UserName,
                AvatarUser = x.Avatar ?? "",
                ParentUserName = x.Comment.ParentId.HasValue
                    ? userByCommentId.GetValueOrDefault(x.Comment.ParentId.Value)
                    : null,
                Children = new List<CommentResponseDto>()
            }).ToList();

            // 4. Build tree
            var tree = BuildCommentTree(dtoList);

            // 5. Pagination root nodes
            var totalRoot = flat.Count;

            var paginatedRoots = tree
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<CommentResponseDto>
            {
                Total = totalRoot,
                Page = page,
                PageSize = pageSize,
                Data = paginatedRoots
            };
        }

        public async Task<int> GetTotalComments(int songId)
        {
            int total = await _db.Comments
                                      .Where(c => c.SongId == songId)
                                      .CountAsync();

            return total;
        }


        public async Task<CommentResponseDto?> UpdateAsync(int id, UpdateCommentDto dto)
        {
            var comment = await _db.Comments.FindAsync(id);
            if (comment == null || comment.IsDeleted) return null;

            comment.Content = dto.Content;
            comment.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return comment.ToCommentDto();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var comment = await _db.Comments.FindAsync(id);
            if (comment == null || comment.IsDeleted) return false;

            comment.IsDeleted = true;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
