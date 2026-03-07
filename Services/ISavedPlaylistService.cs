using MUMbackend.Dtos;
using MUMbackend.Models;

namespace MUMbackend.Services
{
    public interface ISavedPlaylistService
    {
        Task<bool> ToggleSaveAsync(int userId, int playlistId);
        Task<bool> IsSavedAsync(int userId, int playlistId);
        Task<List<PlaylistResponseDto>> GetSavedPlaylistsAsync(int userId);
    }
}
