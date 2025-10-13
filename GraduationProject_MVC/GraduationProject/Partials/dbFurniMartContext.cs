using Microsoft.EntityFrameworkCore;

namespace ApiProject.Models
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

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // ⭐ 在這裡加入自訂的 Entity 設定
            // 這些設定不會被 Scaffold 覆蓋

            // 範例：設定索引
            modelBuilder.Entity<TProduct>()
                .HasIndex(p => p.FName)
                .HasDatabaseName("IX_Product_Name");

            // 範例：設定預設值
            modelBuilder.Entity<TProduct>()
                .Property(p => p.FCreateTime)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<TProductVariant>()
                .Property(v => v.FCreateTime)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<TProductAsset>()
                .Property(a => a.FCreateTime)
                .HasDefaultValueSql("GETDATE()");

            // 範例：設定唯一約束
            modelBuilder.Entity<TProductVariant>()
                .HasIndex(v => v.FSku)
                .IsUnique()
                .HasDatabaseName("IX_ProductVariant_SKU_Unique");
        }
    }
}
