using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MUMbackend.Models;
using MUMbackend.Services;
using System.Security.Claims;

namespace MUMbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // POST: api/follows
        [HttpPost("follow/{followingId}")]
        public async Task<IActionResult> Follow(int followingId)
        {
            await _followService.FollowAsync(CurrentUserId, followingId);
            return Ok(new { status = 200, message = "Follow thành công" });
        }

        // DELETE: api/follows/{followingId}
        [HttpDelete("Unfollow/{followingId}")]
        public async Task<IActionResult> Unfollow(int followingId)
        {
            await _followService.UnfollowAsync(CurrentUserId, followingId);
            return Ok(new { status = 200, message = "Unfollow thành công" });
        }

        // GET: api/follows/is-following/{userId}
        [HttpGet("is-following/{userId}")]
        public async Task<IActionResult> IsFollowing(int userId)
        {
            var result = await _followService.IsFollowingAsync(CurrentUserId, userId);
            return Ok(result);
        }

        
        [HttpGet("GetFollowers/{userId}")]
        public async Task<IActionResult> GetFollowers(int userId)
        {
            return Ok(await _followService.GetFollowersAsync(userId));
        }

        [HttpGet("GetFollowing/{userId}")]
        public async Task<IActionResult> GetFollowing(int userId)
        {
            return Ok(await _followService.GetFollowingAsync(userId));
        }
    }

}
