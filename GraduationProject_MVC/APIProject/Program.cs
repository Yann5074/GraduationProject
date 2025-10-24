using ApiProject.Infrastructure; // ✅ SessionAuthHandler 的命名空間
using ApiProject.Interfaces;
using ApiProject.Models;
using ApiProject.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

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
        policy.WithOrigins("http://localhost:5173") // 你的前端埠號
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // ✅ 必須，才能帶 .AspNetCore.Session
    });
});

// DbContext（只保留一次）
builder.Services.AddDbContext<dbFurniMartContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbFurniMart")));

// DI（各一次）
builder.Services.AddScoped<IOrderService, COrderService>();
builder.Services.AddScoped<ICartService, CCartService>();
builder.Services.AddScoped<IMemberService, CMemberServices>();
builder.Services.AddScoped<IPasswordHasher<TMember>, PasswordHasher<TMember>>();
// 加入 CMemberAuthService
builder.Services.AddScoped<IHelpToolService, CMemberAuthService>();

// ✅ Session 需要「分散式快取」
builder.Services.AddDistributedMemoryCache();

// ✅ 啟用 Session
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.Cookie.SameSite = SameSiteMode.None;         // 跨站 XHR 必須 None
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ✅ 必須 Secure，否則瀏覽器會丟棄 Cookie
});


// ✅ 只用 Session 作為驗證方案（不要再同時註冊 Cookie Auth）
builder.Services.AddAuthentication("SessionAuth")
    .AddScheme<AuthenticationSchemeOptions, SessionAuthHandler>("SessionAuth", _ => { });

builder.Services.AddAuthorization();

// 讓 Service 可取到 HttpContext（若有用到）
builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped<IProductService, CProductService>();

var app = builder.Build();

// Swagger（開發用）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
var graduationProjectImagesPath = Path.Combine(
    Directory.GetCurrentDirectory(),           // ApiProject 目錄
    "..",                                       // 上一層（方案目錄）
    "GraduationProject",                       // GraduationProject 專案
    "wwwroot",                                 // wwwroot 資料夾
    "ProductImages"                            // ProductImages 資料夾
);

Console.WriteLine($"當前目錄: {Directory.GetCurrentDirectory()}");
Console.WriteLine($"計算路徑: {graduationProjectImagesPath}");
Console.WriteLine($"路徑存在: {Directory.Exists(graduationProjectImagesPath)}");

if (Directory.Exists(graduationProjectImagesPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(graduationProjectImagesPath),
        RequestPath = "/ProductImages"
    });
    Console.WriteLine($"產品圖片路徑: {graduationProjectImagesPath}");

    // 顯示前幾個檔案
    var files = Directory.GetFiles(graduationProjectImagesPath).Take(3);
    foreach (var file in files)
    {
        Console.WriteLine($"   - {Path.GetFileName(file)}");
    }
}
else
{
    Console.WriteLine($"找不到圖片資料夾: {graduationProjectImagesPath}");
}

// CORS（在 Auth 之前即可）
app.UseCors("VueClient");


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

// 靜態檔案（wwwroot）
app.UseStaticFiles();
Console.WriteLine($"預設靜態檔案: wwwroot/");

// ✅ 順序：Session → Authentication → Authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("==========================================");
Console.WriteLine($"🌐 前台 API 運行於: {app.Urls.FirstOrDefault()}");
Console.WriteLine($"📂 產品圖片: /ProductImages/");
Console.WriteLine($"📂 會員頭像: /MemberHeadImages/");
Console.WriteLine($"🔗 測試範例:");
Console.WriteLine($"   {app.Urls.FirstOrDefault()}/ProductImages/a.webp");
Console.WriteLine($"   {app.Urls.FirstOrDefault()}/MemberHeadImages/user1.jpg");
Console.WriteLine("==========================================");

app.Run();
