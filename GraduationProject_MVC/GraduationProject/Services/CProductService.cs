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

        public async Task<CProductDTO> CreateAsync(CProductCreateDTO input, CancellationToken ct = default)
        {
            var entity = new TProduct
            {
                FName = input.Name,
                FDescription = input.Description,
                FCategoryId = input.CategoryId,
                FPstatus = input.PStatus,
                FDiscount = input.Discount,
                FCreateTime = DateTime.UtcNow,
                FUpdateTime = DateTime.UtcNow
            };

            _db.TProducts.Add(entity);
            await _db.SaveChangesAsync(ct);

            return MapToDto(entity);
        }

        public async Task<CProductDTO> UpdateAsync(CProductUpdateDTO input, CancellationToken ct = default)
        {
            var entity = await _db.TProducts.FirstOrDefaultAsync(x => x.FProductId == input.ProductId, ct);
            if (entity == null)
                throw new KeyNotFoundException($"Product {input.ProductId} not found.");

            entity.FName = input.Name;
            entity.FDescription = input.Description;
            entity.FCategoryId = input.CategoryId;
            entity.FPstatus = input.PStatus;
            entity.FDiscount = input.Discount;
            entity.FUpdateTime = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return MapToDto(entity);
        }

        public async Task<bool> DeleteAsync(int productId, CancellationToken ct = default)
        {
            var entity = await _db.TProducts.FirstOrDefaultAsync(x => x.FProductId == productId, ct);
            if (entity == null) return false;

            _db.TProducts.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
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
