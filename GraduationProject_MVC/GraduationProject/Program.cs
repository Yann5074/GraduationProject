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

// ¤¶­±¹ïÀ³¹ê§@¡]ÃöÁä¤@¦æ¡^
builder.Services.AddScoped<IEmployeeService, CEmployeeService>();

builder.Services.AddControllersWithViews();

//ï¿½`ï¿½Jï¿½ï¿½Æ®wï¿½sï¿½u
//ï¿½ï¿½ï¿½U (ï¿½`ï¿½J)
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Order}/{action=List}/{id?}");
app.MapRazorPages();

app.MapControllers(); // ï¿½ï¿½ [ApiController] ï¿½ï¿½ï¿½Ñ¥Í®ï¿½

app.Run();
