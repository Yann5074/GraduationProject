using ApiProject.Interfaces;
using ApiProject.Middleware;
using ApiProject.Models;
using ApiProject.Services;
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
            "http://localhost:5173" //測試環境未來可能補上線環境
            )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<dbFurniMartContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("dbFurniMart")));

// 注入 COrderService
builder.Services.AddScoped<IOrderService, COrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//設定例外錯誤的中介層
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("VueClient");
app.UseAuthorization();

app.MapControllers();

app.Run();
