using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace GraduationProject.Models
{
    public partial class dbFurniMartContext : DbContext
    {
        public dbFurniMartContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfiguration Config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
                optionsBuilder.UseSqlServer(Config.GetConnectionString("dbFurniMart"));
            }
        }

        // ✅ 正確位置：在「方法裡」呼叫 partial
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    OnModelCreatingPartial(modelBuilder);  // ← 這行要在方法內
        //}

        // ✅ 宣告 partial method（只能宣告一次）
        //partial void OnModelCreatingPartial(ModelBuilder modelBuilder);



    }
}
