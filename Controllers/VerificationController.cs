using Microsoft.AspNetCore.Mvc;
using MUMbackend.Data;
using MUMbackend.Models;
using MUMbackend.Services.Auth;
using Microsoft.EntityFrameworkCore;
using MUMbackend.Dtos;
using MUMbackend.Mappers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace MUMbackend.Controllers
{
    [ApiController]
    [Route("api/Verification/[action]")]
    public class VerificationController : ControllerBase
    {
        private readonly VerificationService _verificationService;
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;

        public VerificationController(VerificationService verificationService, AppDbContext context, PasswordService passwordService)
        {
            _verificationService = verificationService;
            _context = context;
            _passwordService = passwordService;
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOtpRegister(String Code, UserCreateDto userCreateDto)
        {
            if (await _verificationService.VerifyCodeAsync(userCreateDto.Email, Code))
            {
                // nếu hợp lệ, kích hoạt user
                
                    var user = UserMapper.ToEntityCreate(userCreateDto);
                    user.Password = _passwordService.Hash(userCreateDto.Password);
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                

                return Ok(new ApiResponse<object>(true, "Xác nhận thành công, tài khoản đã được kích hoạt!"));
            }

            return BadRequest(new ApiResponse<object>(false, "Mã xác nhận không hợp lệ hoặc đã hết hạn!"));
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOtpForgotPass(string Email, String Code)
        {
            var (success, claims) = await _verificationService.VerifyCodeGenToken(Email, Code);
            
            if (success)
            {
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Cookie persistent
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60) // Thời gian hết hạn
                };
                // ✅ Đăng nhập và tạo cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);
                return Ok(new 
                { 
                    success = true, 
                    message = "Xác nhận thành công!",
                });
            }

            return BadRequest(new ApiResponse<object>(false, "Mã xác nhận không hợp lệ hoặc đã hết hạn!"));
        }

    }
}
