using ApiProject.Hubs;
using ApiProject.Infrastructure; // ✅ SessionAuthHandler 的命名空間
using ApiProject.Interfaces;
using ApiProject.Models;
using ApiProject.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSignalR();
builder.Services.AddSingleton<ChatClient>(serviceProvider =>
{
    var key = "api-key";
    var model = "gpt-4o";

    return new ChatClient(model, key);
});


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
        policy.WithOrigins("http://localhost:5173", "https://localhost:7131", "https://localhost:7093") // 你的前端埠號
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
builder.Services.AddScoped<IAiProductService, AiProductService>();
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



var app = builder.Build();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".glb"] = "model/gltf-binary";
provider.Mappings[".gltf"] = "model/gltf+json";
provider.Mappings[".bin"] = "application/octet-stream";
provider.Mappings[".hdr"] = "application/octet-stream";
provider.Mappings[".ktx2"] = "image/ktx2";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = true
});

//singlR即時通訊
app.UseCors();
app.MapHub<ChatHub>("/chatHub");
app.MapControllers();

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

var allowedOrigins = new[] {
    "http://localhost:5173",
    "https://localhost:5173",
    "http://localhost:5174",
    "https://localhost:5174"
};

var productImagesPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "GraduationProject", "wwwroot", "ProductImages")
);
Directory.CreateDirectory(productImagesPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(productImagesPath),
    RequestPath = "/ProductImages",
    ContentTypeProvider = provider,   // ← 用你上面那個 provider，讓 .glb/.gltf/.ktx2/.hdr MIME 正確

    OnPrepareResponse = ctx =>
    {
        var origin = ctx.Context.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin))
        {
            ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            ctx.Context.Response.Headers["Vary"] = "Origin"; // 讓快取分開
                                                             // 如果你真的需要帶 cookie 再開這行；一般抓模型不需要：
                                                             // ctx.Context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
        }
    }

});

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
    RequestPath = "/MemberHeadImages",

});

// 靜態檔案（wwwroot）
app.UseStaticFiles();
Console.WriteLine($"預設靜態檔案: wwwroot/");

// ✅ 順序：Session → Authentication → Authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
