using MUMbackend.Dtos;
using MUMbackend.Models;
using System.Text.RegularExpressions;

namespace MUMbackend.Mappers
{
    public static class GenreMapper
    {
        public static GenreDto ToDto(Genre genre)
        {
            return new GenreDto
            {
                Id = genre.Id,
                Name = genre.Name,
                Slug = genre.Slug
            };
        }

        public static Genre ToEntityCreate(GenreDto dto)
        {
            return new Genre
            {
                Name = dto.Name,
                Slug = dto.Slug
            };
        }

        public static void ToEntityUpdate(Genre genre, GenreDto dto)
        {
            genre.Name = dto.Name ?? genre.Name;
            genre.Slug = dto.Slug ?? GenerateSlug(dto.Name ?? genre.Name);
        }

        // Helper method để tạo slug từ name
        private static string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // Chuyển sang lowercase và loại bỏ dấu tiếng Việt
            string slug = RemoveVietnameseTones(name.ToLower());

            // Thay thế khoảng trắng và ký tự đặc biệt bằng dấu gạch ngang
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }

        // Helper method để loại bỏ dấu tiếng Việt
        private static string RemoveVietnameseTones(string text)
        {
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
            }

            return text;
        }
    }
}