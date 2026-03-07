using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MUMbackend.Services;

[Route("api/saved-playlists/[action]")]
[ApiController]
[Authorize]
public class SavedPlaylistsController : ControllerBase
{
    private readonly ISavedPlaylistService _service;

    public SavedPlaylistsController(ISavedPlaylistService service)
    {
        _service = service;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
    }

    [HttpPost("{playlistId}")]
    public async Task<IActionResult> ToggleSave(int playlistId)
    {
        var userId = GetUserId();

        var result = await _service.ToggleSaveAsync(userId, playlistId);

        return Ok(new { isSaved = result });
    }

    [HttpGet("{playlistId}")]
    public async Task<IActionResult> IsSaved(int playlistId)
    {
        var userId = GetUserId();

        var isSaved = await _service.IsSavedAsync(userId, playlistId);

        return Ok(new { isSaved });
    }

    [HttpGet]
    public async Task<IActionResult> GetMySavedPlaylists()
    {
        var userId = GetUserId();

        var playlists = await _service.GetSavedPlaylistsAsync(userId);

        return Ok(playlists);
    }
}