using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Dtos;
using MUMbackend.Models;

namespace MUMbackend.Services
{
    public class ReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasUserReportedAsync(int userId, int targetId)
        {
            return await _context.Reports
                .AnyAsync(r => r.ReporterId == userId && r.TargetId == targetId);

        }

        public async Task<Report> CreateReportAsync(int userId, CreateReportDto dto)
        {
            var already = await HasUserReportedAsync(userId, dto.TargetId);

            if (already)
                throw new Exception("You have already reported this item.");

            var report = new Report
            {
                ReporterId = userId,
                TargetId = dto.TargetId,
                TargetType = dto.TargetType,
                Reason = dto.Reason
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<PagedResponse<ReportResponseDto>> GetAllReportsAsync(
     int page = 1,
     int pageSize = 25,
     string? status = null)
        {
            var query = _context.Reports
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            query = query.OrderByDescending(r => r.CreatedAt);

            var totalRecords = await query.CountAsync();

            var reports = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReportResponseDto
                {
                    Id = r.Id,
                    ReporterId = r.ReporterId,

                    // Email của người report
                    ReporterEmail = _context.Users
                        .Where(u => u.Id == r.ReporterId)
                        .Select(u => u.Email)
                        .FirstOrDefault(),

                    TargetType = r.TargetType,
                    TargetId = r.TargetId,

                    // Name của đối tượng bị report
                    TargetName =
                        r.TargetType == "Song"
                            ? _context.Songs
                                .Where(s => s.Id == r.TargetId)
                                .Select(s => s.Title)
                                .FirstOrDefault()
                        : r.TargetType == "Playlist"
                            ? _context.Playlists
                                .Where(p => p.Id == r.TargetId)
                                .Select(p => p.Name)
                                .FirstOrDefault()
                        : r.TargetType == "User"
                            ? _context.Users
                                .Where(u => u.Id == r.TargetId)
                                .Select(u => u.Email)
                                .FirstOrDefault()
                        : null,

                    Reason = r.Reason,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return new PagedResponse<ReportResponseDto>(
                reports,
                page,
                pageSize,
                totalRecords
                
            );
        }

        public async Task<bool> UpdateStatusAsync(int reportId, string status, int adminId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return false;

            report.Status = status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResponse<ReportResponseDto>> SmartSearchReportsAsync(
    string? keyword,
    int page = 1,
    int pageSize = 25)
        {
            var query = _context.Reports.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();

                // STATUS
                if (keyword == "pending" || keyword == "solved" || keyword == "rejected")
                {
                    query = query.Where(r => r.Status.ToLower() == keyword);
                }

                // TARGET TYPE
                else if (keyword == "song" || keyword == "playlist" || keyword == "user")
                {
                    query = query.Where(r => r.TargetType.ToLower() == keyword);
                }

                // DATE RANGE (vd: 2026-03-01 to 2026-03-10)
                else if (keyword.Contains("to"))
                {
                    var parts = keyword.Split("to");

                    if (DateTime.TryParse(parts[0], out var fromDate) &&
                        DateTime.TryParse(parts[1], out var toDate))
                    {
                        query = query.Where(r =>
                            r.CreatedAt >= fromDate &&
                            r.CreatedAt <= toDate);
                    }
                }

                // SINGLE DATE
                else if (DateTime.TryParse(keyword, out var date))
                {
                    query = query.Where(r =>
                        r.CreatedAt.Date == date.Date);
                }

                // TEXT SEARCH (reason / email)
                else
                {
                    query = query.Where(r =>
                        r.Reason.ToLower().Contains(keyword) ||
                        _context.Users
                            .Where(u => u.Id == r.ReporterId)
                            .Select(u => u.Email.ToLower())
                            .FirstOrDefault()
                            .Contains(keyword)
                    );
                }
            }

            query = query.OrderByDescending(r => r.CreatedAt);

            var totalRecords = await query.CountAsync();

            var reports = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReportResponseDto
                {
                    Id = r.Id,
                    ReporterId = r.ReporterId,

                    ReporterEmail = _context.Users
                        .Where(u => u.Id == r.ReporterId)
                        .Select(u => u.Email)
                        .FirstOrDefault(),

                    TargetType = r.TargetType,
                    TargetId = r.TargetId,

                    TargetName =
                        r.TargetType == "Song"
                            ? _context.Songs
                                .Where(s => s.Id == r.TargetId)
                                .Select(s => s.Title)
                                .FirstOrDefault()
                        : r.TargetType == "Playlist"
                            ? _context.Playlists
                                .Where(p => p.Id == r.TargetId)
                                .Select(p => p.Name)
                                .FirstOrDefault()
                        : r.TargetType == "User"
                            ? _context.Users
                                .Where(u => u.Id == r.TargetId)
                                .Select(u => u.Username)
                                .FirstOrDefault()
                        : null,

                    Reason = r.Reason,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return new PagedResponse<ReportResponseDto>(
                reports,
                totalRecords,
                page,
                pageSize
            );
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);

            if (report == null)
                return false;

            _context.Reports.Remove(report);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}