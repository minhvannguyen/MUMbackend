using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MUMbackend.Dtos;
using MUMbackend.Models;
using MUMbackend.Services;

[Route("api/recommendation/[action]")]
[ApiController]
public class RecommendationController : ControllerBase
{
    private readonly RecommendationService _service;

    public RecommendationController(RecommendationService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get25Songs(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 25)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var result = await _service.GetRecommendationsAsync(userId, page, pageSize);

        return Ok(ApiResponse<PagedResponse<SongDto>>
            .Ok("Lấy recommendation thành công", result));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetSongRecommend(
    [FromQuery] double? score,
    [FromQuery] int? songId,
    [FromQuery] int limit = 20)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var result = await _service.GetRecommendationsAsync(
            userId,
            score,
            songId,
            limit);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetGuestRecommend(
    [FromQuery] double? score,
    [FromQuery] int? songId,
    [FromQuery] int limit = 20)
    {
        var result = await _service.GetGuestRecommendationsAsync(score, songId, limit);

        return Ok(new
        {
            items = result.Items,
            nextScore = result.NextScore,
            nextSongId = result.NextId,
            hasMore = result.HasMore
        });
    }
}