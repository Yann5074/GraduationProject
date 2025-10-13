using GraduationProject.Data;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 介面對應實作（關鍵一行）
builder.Services.AddScoped<IEmployeeService, CEmployeeService>();
builder.Services.AddScoped<IPasswordHasher<TEmployee>, PasswordHasher<TEmployee>>();
builder.Services.AddScoped<IAuthService, CAuthService>();

builder.Services.AddControllersWithViews();
builder.Services.AddSession(o =>
{
    // Session 過期時間 → 4 小時沒動作就失效
    o.IdleTimeout = TimeSpan.FromHours(4);

    // Cookie 只能透過 HTTP 存取（瀏覽器 JS 讀不到）
    // 防止 XSS 攻擊
    o.Cookie.HttpOnly = true;

    // 告訴 GDPR / Cookie 同意機制：這顆 Cookie 是「必須要有」的
    o.Cookie.IsEssential = true;
});

//嚙窯嚙皚嚙踝蕭おw嚙編嚙線
//嚙踝蕭嚙磊 (嚙窯嚙皚)
builder.Services.AddDbContext<dbFurniMartContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbFurniMart"));
});

//嚙窯嚙皚OrderService
builder.Services.AddScoped<IOrderService, COrderService>();
builder.Services.AddScoped<IOrderDetailService, COrderDetailService>();
// Application services
builder.Services.AddScoped<IMemberService, CMemberService>();

// 嚙踝蕭嚙磊 SKU 嚙談佗蕭嚙踝蕭嚙璀嚙踝蕭
builder.Services.AddScoped<SkuGenerator>();
// Application services
builder.Services.AddScoped<IProductService, CProductService>();

var app = builder.Build();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".webp"] = "image/webp";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Order}/{action=List}/{id?}");
app.MapRazorPages();

app.MapControllers(); // 嚙踝蕭 [ApiController] 嚙踝蕭嚙諸生殷蕭

app.Run();
