using Microsoft.AspNetCore.Hosting;

namespace MUMbackend.Services
{
    public interface IFileService
    {
        Task<string> UploadImageAsync(IFormFile file, string subFolder = "profileImages");
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string subFolder = "profileImages")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ!");

            // 🗂️ Nếu WebRootPath null, sử dụng ContentRootPath và tạo wwwroot
            string basePath = _env.WebRootPath;
            if (string.IsNullOrEmpty(basePath))
            {
                basePath = Path.Combine(_env.ContentRootPath, "wwwroot");
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);
            }

            var uploadPath = Path.Combine(basePath, "uploads", subFolder);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // 🧾 Tạo tên file duy nhất
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            // 💾 Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 🌐 Trả về đường dẫn tương đối cho client
            return $"/uploads/{subFolder}/{uniqueFileName}";
        }
    }
}
