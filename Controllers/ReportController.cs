using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MUMbackend.Dtos;
using MUMbackend.Models;
using MUMbackend.Services;

namespace MUMbackend.Controllers
{
    [Route("api/reports/[action]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _service;

        public ReportsController(ReportService service)
        {
            _service = service;
        }

        // =============================
        // Create report
        // =============================
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReport(CreateReportDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            try
            {
                var report = await _service.CreateReportAsync(userId, dto);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // =============================
        // Check user reported
        // =============================
        [Authorize]
        [HttpGet("check/{targetId}")]
        public async Task<IActionResult> Check(int targetId)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _service.HasUserReportedAsync(userId, targetId);

            return Ok(result);
        }

        // =============================
        // Admin: Get all
        // =============================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 25,
    [FromQuery] string? status = null)
        {
            var reports = await _service.GetAllReportsAsync(page, pageSize, status);

            return Ok(ApiResponse<PagedResponse<ReportResponseDto>>
                .Ok("Lấy danh sách báo cáo thành công", reports));
        }

        // =============================
        // Admin: Update status
        // =============================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateReportStatusDto dto)
        {
            var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var success = await _service.UpdateStatusAsync(id, dto.Status, adminId);

            if (!success)
                return NotFound();

            return Ok();
        }
    }
}