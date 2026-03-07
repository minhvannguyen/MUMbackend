using MUMbackend.Models;
using MUMbackend.Dtos.Comments;

namespace MUMbackend.Mappers
{
    public static class CommentMapper
    {
        public static CommentResponseDto ToCommentDto(this Comment comment)
        {
            return new CommentResponseDto
            {
                Id = comment.Id,
                UserId = comment.UserId,
                SongId = comment.SongId,
                Content = comment.IsDeleted ? "Bình luận đã bị xoá" : comment.Content,
                ParentId = comment.ParentId,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                IsDeleted = comment.IsDeleted
            };
        }
    }
}
