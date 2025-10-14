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

        public async Task<ResultPagedDTO<ResProductDTO>> GetProductsAsync(ReqProductQueryDTO query, CancellationToken ct = default)
        {
            // base query（只讀 + 關聯）
            var q = _db.TProducts
                .AsNoTracking()
                .Include(p => p.Assets)
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .AsQueryable();

            // 篩選
            if (!string.IsNullOrWhiteSpace(query.Q))
            {
                var kw = query.Q.Trim();
                q = q.Where(p => (p.Name ?? "").Contains(kw) || (p.Description ?? "").Contains(kw));
            }
            if (query.CategoryId is { } catId)
                q = q.Where(p => p.CategoryId == catId);

            if (query.Status is { } status)
                q = q.Where(p => p.PStatus == status);

            if (query.ColorId is { } colorId)
                q = q.Where(p => p.Variants.Any(v => v.ColorId == colorId));

            if (query.MinPrice is { } minp)
                q = q.Where(p => p.Variants.Any(v => v.Price >= minp));

            if (query.MaxPrice is { } maxp)
                q = q.Where(p => p.Variants.Any(v => v.Price <= maxp));

            // 排序（先準備投影需要的價格範圍）
            var projected = q.Select(p => new
            {
                p.ProductId,
                p.Name,
                p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                PrimaryImageUrl = p.Assets
                    .OrderByDescending(a => a.IsPrimary).ThenBy(a => a.SortOrder)
                    .Select(a => a.Url)
                    .FirstOrDefault(),
                MinPrice = p.Variants.Min(v => v.Price),
                MaxPrice = p.Variants.Max(v => v.Price),
                CreatedTime = p.ProductId // 若有 fCreateTime 可改為該欄位
            });

            // 動態排序
            projected = (query.SortBy?.ToLowerInvariant(), query.SortDir?.ToLowerInvariant()) switch
            {
                ("price", "desc") => projected.OrderByDescending(x => x.MinPrice ?? 0),
                ("price", _) => projected.OrderBy(x => x.MinPrice ?? 0),
                ("name", "desc") => projected.OrderByDescending(x => x.Name),
                ("name", _) => projected.OrderBy(x => x.Name),
                ("newest", "asc") => projected.OrderBy(x => x.CreatedTime),
                ("newest", _) => projected.OrderByDescending(x => x.CreatedTime),
                _ => projected.OrderBy(x => x.ProductId)
            };

            // 分頁
            var total = await projected.CountAsync(ct);
            var items = await projected
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new ResProductDTO
                {
                    ProductId = x.ProductId,
                    Name = x.Name,
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName,
                    MinPrice = x.MinPrice,
                    MaxPrice = x.MaxPrice,
                    PrimaryImageUrl = x.PrimaryImageUrl
                })
                .ToListAsync(ct);

            return new ResultPagedDTO<ResProductDTO>
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = total,
                Items = items
            };
        }


    }
}

