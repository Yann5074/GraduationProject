using Microsoft.Extensions.FileProviders;
using ApiProject.Interfaces;
using ApiProject.Models;
using ApiProject.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC/Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS：允許前端帶 Cookie（Session）
builder.Services.AddCors(option =>
{
    option.AddPolicy("VueClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();  // ← 必須，才能帶 .AspNetCore.Session
    });
});

// DbContext（保留一次即可）
builder.Services.AddDbContext<dbFurniMartContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbFurniMart")));

// DI
builder.Services.AddScoped<IOrderService, COrderService>();
// 加入 CCartService
builder.Services.AddScoped<ICartService, CCartService>();
// CMemberServices
builder.Services.AddScoped<IMemberService, CMemberServices>();
builder.Services.AddScoped<IPasswordHasher<TMember>, PasswordHasher<TMember>>();

// ✅ Session 需要「分散式快取」實作
builder.Services.AddDistributedMemoryCache();

// ✅ 啟用 Session
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;

    // ★ 跨站 XHR 需要這兩行
    o.Cookie.SameSite = SameSiteMode.None;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 需 HTTPS
});

// ✅ 只用 Session 作為驗證方案（重點：不要同時再呼叫 Cookie 的 AddAuthentication）
builder.Services.AddAuthentication("SessionAuth")
    .AddScheme<AuthenticationSchemeOptions, SessionAuthHandler>("SessionAuth", _ => { });

builder.Services.AddAuthorization();

// 讓 Service 可取到 HttpContext（若有用到）
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<dbFurniMartContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbFurniMart"));
});

builder.Services.AddDbContext<dbFurniMartContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbFurniMart"));
});

var app = builder.Build();

// Swagger（開發用）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS（在 Auth 之前即可）
app.UseCors("VueClient");

// 靜態檔案（wwwroot）
app.UseStaticFiles();

// 映射共用圖片資料夾：<方案根>/SharedStorage/MemberHeadImages
var apiSharedImagesPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "SharedStorage", "MemberHeadImages")
);
Directory.CreateDirectory(apiSharedImagesPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(apiSharedImagesPath),
    RequestPath = "/MemberHeadImages"
});

// ✅ 順序：Session → Authentication → Authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
