using ApiProject.Interfaces;
using ApiProject.Models;
using ApiProject.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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

// �`�J COrderService
builder.Services.AddScoped<IOrderService, COrderService>();
// CMemberServices
builder.Services.AddScoped<IMemberService, CMemberServices>();

builder.Services.AddScoped<IPasswordHasher<TMember>, PasswordHasher<TMember>>();

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
app.UseAuthorization();

app.MapControllers();

app.Run();
