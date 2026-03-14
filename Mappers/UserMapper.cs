using Microsoft.AspNetCore.Identity;
using MUMbackend.Dtos;
using MUMbackend.Models;
using MUMbackend.Services;

namespace MUMbackend.Mappers
{
    public static class UserMapper
    {
        private static readonly PasswordHasher<User> _passwordHasher = new();

        public static UserDto ToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
            };
        }

        public static void ToEntityUpdate(User user, UserDto dto)
        {
            user.Username = dto.Username;
            user.Bio = dto.Bio;
            user.Role = dto.Role;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.Now;
        }

        public static User ToEntityCreate(UserCreateDto dto)
        {
            return new User
            {
                Username = dto.Username,
                Email = dto.Email,
                AvatarUrl = dto.AvatarUrl,
                Bio = dto.Bio,
                CreatedAt = DateTime.Now,
            };
        }

        public static User ToEntity(UserDto dto)
        {
            return new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = dto.Password,
                AvatarUrl = dto.AvatarUrl,
                Bio = dto.Bio,
                Role = dto.Role,
                UpdatedAt = DateTime.Now,
            };
        }

    }
}
