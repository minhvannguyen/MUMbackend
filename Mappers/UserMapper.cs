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
            if (!string.IsNullOrEmpty(dto.Username))
                user.Username = dto.Username;

            if (!string.IsNullOrEmpty(dto.Bio))
                user.Bio = dto.Bio;

            // ❗ chỉ update khi có giá trị
            if (!string.IsNullOrEmpty(dto.Role))
                user.Role = dto.Role;

            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;
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
