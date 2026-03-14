using Microsoft.AspNetCore.Mvc;
using MUMbackend.Services;
using MUMbackend.Dtos;
using MUMbackend.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MUMbackend.Controllers
{
    [ApiController]
    [Route("api/playlists/[action]")]
    public class PlaylistController : ControllerBase
    {
        private readonly PlaylistService _playlistService;
        private readonly IMapper _mapper;

        public PlaylistController(PlaylistService playlistService, IMapper mapper)
        {
            _playlistService = playlistService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<PlaylistResponseDto>>>> GetAll(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
        {
            var (playlists, totalItems) = await _playlistService.GetPagedAsync(pageNumber, pageSize);

            var items = _mapper.Map<IEnumerable<PlaylistResponseDto>>(playlists);

            var pagedResponse = new PagedResponse<PlaylistResponseDto>(
                items,
                pageNumber,
                pageSize,
                totalItems
            );

            return Ok(ApiResponse<PagedResponse<PlaylistResponseDto>>.Ok(
                "Playlists retrieved successfully.",
                pagedResponse
            ));
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUserId(
    int userId,
    int page = 1,
    int pageSize = 10)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _playlistService.GetByUserIdAsync(
                userId,
                page,
                pageSize,
                currentUserId);

            if (result.TotalItems == 0)
            {
                return NotFound(new
                {
                    status = 404,
                    message = "Không tìm thấy playlist nào của người dùng này."
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Danh sách playlist của người dùng.",
                pageSize = result.PageSize,
                total = result.TotalItems,
                totalPages = result.TotalPages,
                data = result.Items
            });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var playlist = await _playlistService.GetByIdAsync(id);
            if (playlist == null) return NotFound();

            var result = _mapper.Map<PlaylistResponseDto>(playlist);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] PlaylistCreateDto dto, IFormFile? coverImage,
    [FromServices] IFileService fileService)
        {
            if (coverImage != null)
            {
                dto.CoverUrl = await fileService.UploadImageAsync(coverImage, "playlistCovers");
            }

            var playlist = _mapper.Map<Playlist>(dto);

            try
            {
                var created = await _playlistService.CreateAsync(playlist);
                var result = _mapper.Map<PlaylistResponseDto>(created);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    status = 201,
                    message = "Playlist đã được tạo thành công!",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    status = 400,
                    message = ex.Message
                });
            }
        }



        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromForm] PlaylistUpdateDto dto, IFormFile? coverImage,
    [FromServices] IFileService fileService)
        {
            if (coverImage != null)
            {
                dto.CoverUrl = await fileService.UploadImageAsync(coverImage, "playlistCovers");
            }
            var updated = await _playlistService.UpdateAsync(id, dto);

            if (!updated) return NotFound(new { status = 404, message = "Update playlist không thành công!" });
            return Ok(new
            {
                status = 200,
                message = "Update playlist thành công!",

            });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _playlistService.DeleteAsync(id);
            if (!deleted) return NotFound(new { status = 404, message = "Xoá playlist không thành công!" });
            return Ok(new
            {
                status = 200,
                message = "xoá playlist thành công!",

            });
        }

        [HttpPost]
        public async Task<IActionResult> AddSong([FromBody] PlaylistAddSongDto dto)
        {
            try
            {
                await _playlistService.AddSongToPlaylistAsync(dto.PlaylistId, dto.SongId);

                return Ok(new
                {
                    status = 200,
                    message = "Thêm bài hát vào playlist thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = 400,
                    message = ex.Message
                });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveSong([FromBody] PlaylistAddSongDto dto)
        {
            try
            {
                await _playlistService.RemoveSongFromPlaylistAsync(
                    dto.PlaylistId,
                    dto.SongId);

                return Ok(new
                {
                    status = 200,
                    message = "Xóa bài hát khỏi playlist thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = 400,
                    message = ex.Message
                });
            }
        }

        [HttpGet("{playlistId}")]
        public async Task<IActionResult> GetSongs(int playlistId)
        {
            try
            {
                var songs = await _playlistService.GetSongsInPlaylistAsync(playlistId);

                return Ok(new
                {
                    status = 200,
                    message = "Danh sách bài hát trong playlist.",
                    total = songs.Count(),
                    data = songs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = 400,
                    message = ex.Message
                });
            }
        }
        [HttpGet("top")]
        [AllowAnonymous] // ✔ cho phép guest gọi API
        public async Task<ActionResult<ApiResponse<PagedResponse<object>>>> GetTopPlaylists(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromServices] PlaylistService playlistService = null)
        {
            if (page < 1)
                return BadRequest(ApiResponse<PagedResponse<object>>.Fail("Số trang phải lớn hơn 0!"));

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(ApiResponse<PagedResponse<object>>.Fail("Số phần tử mỗi trang phải từ 1 đến 100!"));

            int? userId = null;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (claim != null)
                {
                    userId = int.Parse(claim.Value);
                }
            }

            var data = await playlistService.GetTopPlaylistsAsync(page, pageSize, userId);

            return Ok(ApiResponse<PagedResponse<object>>.Ok("Lấy top playlist thành công!", data));
        }

        [HttpPost]
        public async Task<IActionResult> IncrementView([FromQuery] int playlistId)
        {
            var success = await _playlistService.IncrementViewAsync(playlistId);

            if (!success)
                return NotFound("Playlist không tồn tại");

            return Ok(new
            {
                status = 200,
                message = "Tăng view playlist thành công"
            });
        }

        [HttpGet]
        public async Task<IActionResult> SearchByName(
    [FromQuery] string keyword,
    [FromQuery] int userId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Keyword không được để trống."
                });
            }

            var response = await _playlistService
                .SearchByNameAsync(keyword, page, pageSize, userId);

            if (!response.Items.Any())
            {
                return NotFound(new
                {
                    status = 404,
                    message = "Không tìm thấy playlist phù hợp."
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Tìm kiếm playlist thành công.",
                page = response.CurrentPage,
                pageSize = response.PageSize,
                totalItems = response.TotalItems,
                totalPages = response.TotalPages,
                data = response.Items
            });
        }
    }
}
