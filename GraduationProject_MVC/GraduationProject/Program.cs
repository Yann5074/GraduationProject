using GraduationProject.Data;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.Options;
using GraduationProject.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Common.Notifications;


var builder = WebApplication.CreateBuilder(args);

//½Õ¾ã Kestrel ½Ð¨D¤j¤p­­¨î
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 200 * 1024 * 1024; // 200 MB
});

//½Õ¾ã FormOptions
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 200 * 1024 * 1024; // 200 MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});


builder.Services.AddControllersWithViews();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

//Employee
builder.Services.AddScoped<IEmployeeService, CEmployeeService>();
//Hasher(Identityï¿½ï¿½ï¿½ï¿½)
builder.Services.AddScoped<IPasswordHasher<TEmployee>, PasswordHasher<TEmployee>>();
//ï¿½nï¿½Jï¿½ï¿½ï¿½ï¿½
builder.Services.AddScoped<IAuthService, CAuthService>();
//ï¿½Hï¿½cï¿½ï¿½ï¿½Ò¬ï¿½ï¿½ï¿½
builder.Services.Configure<CEmployeeEmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmployeeEmailSender, CMailSenderService>();
//ï¿½Ïªï¿½ï¿½ï¿½ï¿½R
builder.Services.AddScoped<IAnalyticsService, CAnalyticsService>();
//ï¿½ï¿½ï¿½oï¿½nï¿½Jï¿½ï¿½ï¿½uï¿½ï¿½ï¿½
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContextService, CUserContextService>();
//ï¿½Ð°ï¿½ï¿½Þ²z
builder.Services.AddScoped<ILeaveService, CLeaveService>();
//ï¿½eï¿½xï¿½ï¿½ï¿½iï¿½]ï¿½w
builder.Services.AddHttpClient("Api", c =>
{
    c.BaseAddress = new Uri("https://your-api-domain/");//ï¿½Ý§ï¿½
});
builder.Services.AddScoped<IAnnouncementService, CAnnouncementService>();
//ï¿½Hï¿½Hï¿½ï¿½ï¿½ï¿½ (Common.Notifications)
builder.Services.AddNotification(builder.Configuration);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(4);

    o.Cookie.HttpOnly = true;

    o.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<dbFurniMartContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbFurniMart"));
});


builder.Services.AddScoped<IOrderService, COrderService>();
builder.Services.AddScoped<IOrderDetailService, COrderDetailService>();
// Application services
builder.Services.AddScoped<IMemberService, CMemberService>();


builder.Services.AddScoped<SkuGenerator>();
// Application services
builder.Services.AddScoped<IProductService, CProductService>();
builder.Services.AddScoped<IPasswordHasher<TMember>, PasswordHasher<TMember>>();

// Application services
//builder.Services.AddScoped<IChatRoom>

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
var mvcSharedImagesPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "SharedStorage", "MemberHeadImages")
);
Directory.CreateDirectory(mvcSharedImagesPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mvcSharedImagesPath),
    RequestPath = "/MemberHeadImages"
});


app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ChatRoom}/{action=Index}");
app.MapRazorPages();

app.MapControllers(); 

app.Run();
