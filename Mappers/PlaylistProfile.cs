using AutoMapper;
using MUMbackend.Models;
using MUMbackend.Dtos;
using Google.Apis.Gmail.v1.Data;

namespace MUMbackend.Mappers
{
    public class PlaylistProfile : AutoMapper.Profile
    {
        public PlaylistProfile()
        {
            // Playlist -> PlaylistResponseDto
            CreateMap<Playlist, PlaylistResponseDto>();

            // PlaylistCreateDto -> Playlist
            CreateMap<PlaylistCreateDto, Playlist>();

            // PlaylistUpdateDto -> Playlist
            CreateMap<PlaylistUpdateDto, Playlist>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
