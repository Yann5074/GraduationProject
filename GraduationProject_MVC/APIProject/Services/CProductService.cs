using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Services
{
    public class CProductService : IProductService
    {
        private readonly dbFurniMartContext _context;
        public CProductService(dbFurniMartContext context)
        {
            _context = context;
        }

        public async Task<List<ResProductDTO>> GetAllProductsAsync()
        {
            var query = _context.TProducts
              .Include(x => x.Category)
              .Include(x => x.PStatus) // 商品狀態
              .Include(x => x.ProductVariants).ThenInclude(v => v.Color)
              .Include(x => x.ProductAssets)
              .Select(o => new ResProductDTO
              {
                  ProductId = o.ProductId.ToString(),
                  CategoryId = o.CategoryId,
                  Name = o.ProductName,
                  PStatus = o.PStatus,
                  PStatusName = o.PStatus.FStatusName,

              });
            return await query.ToListAsync();

        }

    }
}

