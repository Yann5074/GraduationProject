using GraduationProject.Data;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.Options;
using GraduationProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IEmployeeService, CEmployeeService>();
builder.Services.AddScoped<IPasswordHasher<TEmployee>, PasswordHasher<TEmployee>>();
builder.Services.AddScoped<IAuthService, CAuthService>();
builder.Services.Configure<CEmployeeEmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmployeeEmailSender, CMailSenderService>();

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
    pattern: "{controller=Order}/{action=List}/{id?}");
app.MapRazorPages();

app.MapControllers(); 

app.Run();
