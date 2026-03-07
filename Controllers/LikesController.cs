using Microsoft.AspNetCore.Mvc;
using MUMbackend.Dtos;
using MUMbackend.Services;

namespace MUMbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikesController : ControllerBase
    {
        private readonly LikeService _likeService;

        public LikesController(LikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleLike([FromBody] LikeDto dto)
        {
            var liked = await _likeService.ToggleLikeAsync(dto);
            return Ok(new 
            { 
                Status = 200,
                isLiked = liked,
                message = "Đã Like!",
            });
        }

        [HttpGet("is-liked")]
        public async Task<IActionResult> IsLiked(long userId, string targetType, long targetId)
        {
            var result = await _likeService.IsLikedAsync(userId, targetType, targetId);
            return Ok(new { isLiked = result });
        }

        [HttpGet("count")]
        public async Task<IActionResult> CountLikes(string targetType, long targetId)
        {
            var count = await _likeService.CountLikesAsync(targetType, targetId);
            return Ok(new
            {
                Status = 200,
                count,
            });
        }


    }
}
