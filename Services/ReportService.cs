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
                    TargetType = r.TargetType,
                    TargetId = r.TargetId,
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

        public async Task<bool> UpdateStatusAsync(int reportId, string status, int adminId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return false;

            report.Status = status;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}