using Microsoft.Extensions.FileProviders;
using ApiProject.Interfaces;
using ApiProject.Models;
using ApiProject.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(option =>
{
    option.AddPolicy(name: "VueClient", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173" //�������ҥ��ӥi��ɤW�u����
            )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<dbFurniMartContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("dbFurniMart")));

// COrderService
builder.Services.AddScoped<IOrderService, COrderService>();
// 加入 CCartService
builder.Services.AddScoped<ICartService, CCartService>();
// CMemberServices
builder.Services.AddScoped<IMemberService, CMemberServices>();
// 密碼雜湊器
builder.Services.AddScoped<IPasswordHasher<TMember>, PasswordHasher<TMember>>();
// 設定 Cookie 驗證（重點）
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.Cookie.Name = "app.auth";
        opt.Cookie.HttpOnly = true;
        opt.Cookie.SameSite = SameSiteMode.Lax;   // 同站 Swagger 測試可用 Lax；跨站要 None+Secure
        opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        // API 不要 302 轉導
        opt.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; },
            OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; }
        };
    });
// 用記憶體做分散式快取（本機/單機最方便）
builder.Services.AddDistributedMemoryCache();

// builder
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// ③ 用「Session 驗證」作為預設驗證機制
builder.Services.AddAuthentication("Session")
    .AddScheme<AuthenticationSchemeOptions, SessionAuthHandler>("Session", _ => { });

// 用於在 Service 內取得 HttpContext
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();
app.UseCors("VueClient");

// 讓 wwwroot 可被存取（預設用 wwwroot）
app.UseStaticFiles(); // 確保能讀到 /MemberHeadImages/檔名
// 讓 /MemberHeadImages 指到「方案根目錄/SharedStorage/MemberHeadImages」
var apiSharedImagesPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "SharedStorage", "MemberHeadImages")
);
Directory.CreateDirectory(apiSharedImagesPath); // 確保存在

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(apiSharedImagesPath),
    RequestPath = "/MemberHeadImages"
});
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
