using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Services
{
    public class CProductService : IProductService
    {
        private readonly dbFurniMartContext _db;
        public CProductService(dbFurniMartContext db)
        {
            _db = db;
        }

        // Services/CProductService.cs

        public async Task<List<ResProductDTO>> AllProductAsync(CancellationToken ct = default)
        {
            var query = _db.TProducts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductVariants).ThenInclude(v => v.Color)
                .Include(p => p.ProductAssets)
                // 若你把「刪除」標成 PStatus=4，可解除註解以下一行過濾掉：
                // .Where(p => p.PStatus != 4)
                .OrderByDescending(p => p.ProductId)
                .Select(p => new ResProductDTO
                {
                    ProductId = p.FProductId,
                    Name = p.FName,
                    CategoryId = p.FCategoryId,                         // 若你的 DTO 是 int，這裡是 int；若來源是 int? 請改用 ?? 0
                    CategoryName = p.Category != null ? p.Category.FName : null,

                    // 價格區間（無變體時為 null）
                    MinPrice = p.ProductVariants.Min(v => v.FPrice),
                    MaxPrice = p.ProductVariants.Max(v => v.FPrice),

                    // 主圖（先 IsPrimary，再 SortOrder；沒有就 null）
                    PrimaryImageUrl = p.ProductAssets
                                        .OrderByDescending(a => a.FIsPrimary)
                                        .ThenBy(a => a.FSortOrder)
                                        .Select(a => a.FUrl)
                                        .FirstOrDefault(),

                    // 變體清單（含顏色資訊）
                    Variants = p.ProductVariants
                                .OrderBy(v => v.FPrice ?? decimal.MaxValue)
                                .Select(v => new ResProductVariantDTO
                                {
                                    ProductVariantId = v.FProductVariantId,
                                    SKU = v.FSku,
                                    Price = v.FPrice,
                                    Stock = v.FStock,
                                    ColorId = v.FColorId,
                                    ColorName = v.Color != null ? v.Color.FColorName : null,
                                    ColorCode = v.Color != null ? v.Color.FColorCode : null,
                                    SizeLabel = v.FSizeLabel
                                }).ToList(),

                    // 圖片/資產清單
                    Assets = p.ProductAssets
                                .OrderByDescending(a => a.FIsPrimary)
                                .ThenBy(a => a.FSortOrder)
                                .Select(a => new ResProductAssetDTO
                                {
                                    AssetId = a.FAssetId,
                                    Url = a.FUrl,
                                    IsPrimary = a.FIsPrimary,
                                    SortOrder = a.FSortOrder,
                                    MimeType = a.FMimeType
                                }).ToList()
                });

            return await query.ToListAsync(ct);
        }

    }

}


