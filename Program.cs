
using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Services;
using MUMbackend.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.StaticFiles;
using MUMbackend.Hubs;
using Microsoft.AspNetCore.SignalR;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));    

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "MUMbackend API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập token theo dạng: Bearer {token}"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<VerificationService>();
builder.Services.AddScoped<GmailService>();
builder.Services.AddScoped<PlaylistService>();
builder.Services.AddScoped<LikeService, LikeService>();
builder.Services.AddAutoMapper(typeof(Program)); // Tự động quét tất cả Profile
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<ISavedPlaylistService, SavedPlaylistService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
// ✅ Thêm HttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ToxicCommentService>();
builder.Services.AddScoped<DashboardService>();

// ✅ Đăng ký CookieTokenService
builder.Services.AddScoped<CookieTokenService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // nếu dùng https cho FE, thêm "https://localhost:3000"
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MUM.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;      // ✅ luôn là Secure
        options.Cookie.SameSite = SameSiteMode.None;                  // ✅ cần cho cross-site
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.LoginPath = "/api/auth/login";
        options.LogoutPath = "/api/auth/logout";
        options.AccessDeniedPath = "/api/auth/access-denied";

        // ✅ API không redirect, trả mã lỗi phù hợp
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

// Cookie Policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;               // ✅ cho phép None
});

// ✅ Load appsettings.json + appsettings.{Environment}.json + biến môi trường
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var app = builder.Build();

// ✅ Dòng này phải đặt TRƯỚC app.MapControllers()
app.UseCors("AllowFrontend");

// ✅ Thêm Cookie Policy middleware
app.UseCookiePolicy();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ Phục vụ static files từ wwwroot
app.UseStaticFiles();

// ✅ Kích hoạt Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub")
    .RequireAuthorization();

var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".mp3"] = "audio/mpeg";

// Static files trong wwwroot (ví dụ /uploads/songs/...)
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider,
    OnPrepareResponse = ctx =>
    {
        // CORS cho FE
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "http://localhost:3000");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");

        // Streaming range requests cho audio
        ctx.Context.Response.Headers.Append("Accept-Ranges", "bytes");
    }
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


