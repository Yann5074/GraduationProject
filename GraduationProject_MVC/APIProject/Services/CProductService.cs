using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

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

        //列出產品
        public async Task<List<ResProductDTO>> GetAllProductAsync(CancellationToken ct = default)
        {
            var query = _db.TProducts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductVariants).ThenInclude(v => v.Color)
                .Include(p => p.ProductAssets)
                // .Where(p => p.PStatus != 4)
                .OrderByDescending(p => p.ProductId)
                .Select(p => new ResProductDTO
                {
                    ProductId = p.FProductId,
                    Name = p.FName,
                    CategoryId = p.FCategoryId,                        
                    CategoryName = p.Category != null ? p.Category.FName : null,


                    MinPrice = p.ProductVariants.Min(v => v.FPrice),
                    MaxPrice = p.ProductVariants.Max(v => v.FPrice),


                    PrimaryImageUrl = p.ProductAssets
                                        .OrderByDescending(a => a.FIsPrimary)
                                        .ThenBy(a => a.FSortOrder)
                                        .Select(a => a.FUrl)
                                        .FirstOrDefault(),

    
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


        //搜尋產品
        public async Task<List<ResProductDTO>> GetProductByProdNameAsync(string? keyword, CancellationToken ct = default)
        {
            var q = _db.TProducts
        .AsNoTracking()
        .Include(p => p.Category)
        .Include(p => p.ProductVariants)      // + Color 如需：.ThenInclude(v => v.Color)
        .Include(p => p.ProductAssets)
        .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                q = q.Where(p => (p.FName ?? "").Contains(kw));  // 以產品名稱搜尋
            }

            var result = await q
                .OrderByDescending(p => p.ProductId)
                .Select(p => new ResProductDTO
                {
                    ProductId = p.FProductId,
                    Name = p.FName,
                    CategoryId = p.FCategoryId,
                    CategoryName = p.Category != null ? p.Category.FName : null,
                    MinPrice = p.ProductVariants.Min(v => v.FPrice),
                    MaxPrice = p.ProductVariants.Max(v => v.FPrice),
                    PrimaryImageUrl = p.ProductAssets
                                        .OrderByDescending(a => a.FIsPrimary)
                                        .ThenBy(a => a.FSortOrder)
                                        .Select(a => a.FUrl)
                                        .FirstOrDefault(),
                    Variants = p.ProductVariants
                                .OrderBy(v => v.FPrice ?? decimal.MaxValue)
                                .Select(v => new ResProductVariantDTO
                                {
                                    ProductVariantId = v.FProductVariantId,
                                    SKU = v.FSku,
                                    Price = v.FPrice,
                                    Stock = v.FStock,
                                    // ColorId   = v.ColorId,
                                    // ColorName = v.Color?.ColorName,
                                    // ColorCode = v.Color?.ColorCode,
                                    SizeLabel = v.FSizeLabel
                                }).ToList(),
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
                })
                .ToListAsync(ct);

            return result;

        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var p = await _db.TProducts.FirstOrDefaultAsync(p => p.FProductId == id);
            if (p == null) return false;
            p.FPstatus = 4; // 4=刪除
            p.FUpdateTime = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }



    }


}


