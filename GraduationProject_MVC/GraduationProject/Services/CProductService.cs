using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.Queries;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    public class CProductService : IProductService
    {
        private readonly dbFurniMartContext _db;
        public CProductService(dbFurniMartContext db) => _db = db;

        public async Task<CProductDTO?> GetAsync(int productId, CancellationToken ct = default)
        {
            var p = await _db.TProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FProductId == productId, ct);

            return p == null ? null : MapToDto(p);
        }

        public async Task<IReadOnlyList<CProductDTO>> ListAsync(CancellationToken ct = default)
        {
            return await _db.TProducts
                .AsNoTracking()
                .OrderBy(x => x.FProductId)
                .Select(x => new CProductDTO
                {
                    ProductId = x.FProductId,
                    Name = x.FName,
                    Description = x.FDescription,
                    CategoryId = x.FCategoryId,
                    PStatus = x.FPstatus,
                    Discount = x.FDiscount,
                    CreateTime = x.FCreateTime,
                    UpdateTime = x.FUpdateTime
                })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CProductDTO>> SearchAsync(string? keyword, CancellationToken ct = default)
        {
            var query = _db.TProducts.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(p =>
                    (p.FName ?? "").Contains(kw) ||
                    (p.FDescription ?? "").Contains(kw));
            }

            return await query
                .OrderBy(p => p.FProductId)
                .Select(p => new CProductDTO
                {
                    ProductId = p.FProductId,
                    Name = p.FName,
                    Description = p.FDescription,
                    Discount = p.FDiscount,
                    PStatus = p.FPstatus
                })
                .ToListAsync(ct);
        }

        private static CProductDTO MapToDto(TProduct x) => new CProductDTO
        {
            ProductId = x.FProductId,
            Name = x.FName,
            Description = x.FDescription,
            CategoryId = x.FCategoryId,
            PStatus = x.FPstatus,
            Discount = x.FDiscount,
            CreateTime = x.FCreateTime,
            UpdateTime = x.FUpdateTime
        };

        public async Task<ProductDataTableResult> DataTableAsync(ProductDataTableQuery q, CancellationToken ct = default)
        {
            var src = _db.TProducts.AsNoTracking();

            // 總筆數（未過濾）
            int total = await src.CountAsync(ct);

            // 搜尋（Name/Description）
            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var kw = q.Search.Trim();
                src = src.Where(p =>
                    (p.FName ?? "").Contains(kw) ||
                    (p.FDescription ?? "").Contains(kw));
            }

            int filtered = await src.CountAsync(ct);

            // 排序（白名單）
            bool desc = q.SortDir.Equals("DESC", StringComparison.OrdinalIgnoreCase);
            src = q.SortField.ToUpper() switch
            {
                "NAME" => (desc ? src.OrderByDescending(x => x.FName) : src.OrderBy(x => x.FName)),
                "DESCRIPTION" => (desc ? src.OrderByDescending(x => x.FDescription) : src.OrderBy(x => x.FDescription)),
                "DISCOUNT" => (desc ? src.OrderByDescending(x => x.FDiscount) : src.OrderBy(x => x.FDiscount)),
                "PSTATUS" => (desc ? src.OrderByDescending(x => x.FPstatus) : src.OrderBy(x => x.FPstatus)),
                "CREATETIME" => (desc ? src.OrderByDescending(x => x.FCreateTime) : src.OrderBy(x => x.FCreateTime)),
                "UPDATETIME" => (desc ? src.OrderByDescending(x => x.FUpdateTime) : src.OrderBy(x => x.FUpdateTime)),
                _ => (desc ? src.OrderByDescending(x => x.FProductId) : src.OrderBy(x => x.FProductId)),
            };

            // 分頁
            var rows = await src
                .Skip(q.Start)
                .Take(q.Length)
                .Select(x => new CProductDTO
                {
                    ProductId = x.FProductId,
                    Name = x.FName,
                    Description = x.FDescription,
                    Discount = x.FDiscount,
                    PStatus = x.FPstatus,
                    CreateTime = x.FCreateTime,
                    UpdateTime = x.FUpdateTime
                })
                .ToListAsync(ct);

            return new ProductDataTableResult
            {
                TotalRecords = total,
                FilteredRecords = filtered,
                Items = rows
            };
        }
    }
}
