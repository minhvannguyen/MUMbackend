using Microsoft.AspNetCore.Mvc;
using MUMbackend.Services;
using MUMbackend.Dtos.Comments;
using MUMbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace MUMbackend.Controllers
{
    [ApiController]
    [Route("api/comment/[action]")]
    public class CommentController : ControllerBase
    {
        private readonly CommentService _service;
        private readonly ToxicCommentService _toxicCommentService;
        public CommentController(CommentService service, ToxicCommentService toxicCommentService)
        {
            _service = service;
            _toxicCommentService = toxicCommentService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto dto)
        {
            if (_toxicCommentService.IsToxic(dto.Content))
            {
                return BadRequest("Bình luận chứa nội dung không phù hợp.");
            }

            var result = await _service.CreateAsync(dto);
            return Ok(new { status = 200, message = "Created", data = result });
        }

        [HttpGet("song/{songId}")]
        public async Task<IActionResult> GetBySong(
           int songId,
           int page = 1,
           int pageSize = 20
        )
        {
            var result = await _service.GetCommentsForSong(songId, page, pageSize);
            if (result == null)
            {
                return NotFound();
            }
            
            return Ok(ApiResponse<PagedResult<CommentResponseDto>>.Ok("success", result));
        }

        [HttpGet("{songId}")]
        public async Task<IActionResult> GetCountComments(int songId)
        {
            var total = await _service.GetTotalComments(songId);

            return Ok(new
            {
                status = 200,
                message = "Total comments retrieved successfully",
                data = total
            });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (result == null)
                return NotFound(new { status = 404, message = "Comment không tồn tại" });

            return Ok(new { status = 200, data = result });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { status = 404, message = "Comment không tồn tại" });

            return Ok(new { status = 200, message = "Deleted" });
        }
    }
}
