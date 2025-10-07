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

// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½@ï¿½]ï¿½ï¿½ï¿½ï¿½@ï¿½ï¿½^
builder.Services.AddScoped<IEmployeeService, CEmployeeService>();
builder.Services.AddScoped<IPasswordHasher<TEmployee>, PasswordHasher<TEmployee>>();
builder.Services.AddScoped<IAuthService, CAuthService>();

builder.Services.AddControllersWithViews();
builder.Services.AddSession(o =>
{
    // Session ï¿½Lï¿½ï¿½ï¿½É¶ï¿½ ï¿½ï¿½ 4 ï¿½pï¿½É¨Sï¿½Ê§@ï¿½Nï¿½ï¿½ï¿½ï¿½
    o.IdleTimeout = TimeSpan.FromHours(4);

    // Cookie ï¿½uï¿½ï¿½zï¿½L HTTP ï¿½sï¿½ï¿½ï¿½]ï¿½sï¿½ï¿½ï¿½ï¿½ JS Åªï¿½ï¿½ï¿½ï¿½^
    // ï¿½ï¿½ï¿½ï¿½ XSS ï¿½ï¿½ï¿½ï¿½
    o.Cookie.HttpOnly = true;

    // ï¿½iï¿½D GDPR / Cookie ï¿½Pï¿½Nï¿½ï¿½ï¿½ï¿½Gï¿½oï¿½ï¿½ Cookie ï¿½Oï¿½uï¿½ï¿½ï¿½ï¿½ï¿½nï¿½ï¿½ï¿½vï¿½ï¿½
    o.Cookie.IsEssential = true;
});

//µù¥U (ª`¤J)
builder.Services.AddDbContext<dbFurniMartContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbFurniMart"));
});


//ï¿½`ï¿½JOrderService
builder.Services.AddScoped<IOrderService, COrderService>();
builder.Services.AddScoped<IOrderDetailService, COrderDetailService>();
// Application services
builder.Services.AddScoped<IMemberService, CMemberService>();

// ï¿½ï¿½ï¿½U SKU ï¿½Í¦ï¿½ï¿½ï¿½ï¿½Aï¿½ï¿½
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

app.MapControllers(); // ï¿½ï¿½ [ApiController] ï¿½ï¿½ï¿½Ñ¥Í®ï¿½

app.Run();
