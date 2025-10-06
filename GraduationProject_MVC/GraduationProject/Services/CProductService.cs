using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
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
    }
}
