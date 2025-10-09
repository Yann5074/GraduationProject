using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    public class COrderDetailService:IOrderDetailService
    {
        private readonly dbFurniMartContext _context;
        public COrderDetailService(dbFurniMartContext context)
        {
            _context = context;
        }
        public IEnumerable<OrderDetailShowDTO> ShowOrderDetail(int? id)
        {

            var query = _context.TOrderDetails
                .Include(o => o.productVariant)
                .ThenInclude(p => p.product)
                .Select(o => new OrderDetailShowDTO
                {
                    OrderId = o.FOrderId,
                    ProductVariantId = o.FProductVariantId,
                    ProductName = o.productVariant.product.FName,
                    IsDeleted = o.FIsDeleted,
                    UnitPrice = o.FUnitPrice,
                    Quantity = o.FQuantity
                });
            query = query.Where(o => o.OrderId == id && o.IsDeleted != 1);
            return query;
        }

        // 新增訂單明細
         public bool CreateOrderDetail(OrderDetailCreateDTO dtoUi)
        {
            var proId = _context.TProductVariants.FirstOrDefault(o => o.FProductVariantId == dtoUi.ProductVariantId);
            if (proId == null)
            {
                return false;
            }
            var odd = new TOrderDetail
            {
                FOrderId = dtoUi.OrderId,
                FProductVariantId = dtoUi.ProductVariantId,
                FIsDeleted = 0,
                FUnitPrice = (decimal)proId.FPrice, 
                FQuantity = dtoUi.Quantity,
            };
            _context.TOrderDetails.Add(odd);
            _context.SaveChanges();
            return true;
        }
        // 修改訂單明細
         public bool UpdateOrderDetail(OrderDetailUpdateDTO dtoUi)
        {
            var odd = _context.TOrderDetails.FirstOrDefault(od => od.FOrderId == dtoUi.OrderId && od.FProductVariantId == dtoUi.ProductVariantId);
            if (odd == null)
                return false;
            odd.FQuantity = dtoUi.Quantity;
            _context.SaveChanges();

            return true;
        }
        // 刪除訂單明細
        public bool DeleteOrderDetail(int? id)
        {
            if (id == null)
                return false;
            TOrderDetail odd = _context.TOrderDetails.FirstOrDefault(od => od.FProductVariantId == id);
            if (odd == null)
                return false;
            odd.FIsDeleted = 1;
            _context.SaveChanges();
            return true;
        }
        // 搜尋欲修改訂單商品
        public OrderDetailUpdateDTO SearchOrderDetail(int? id)
        {
            var dto = new OrderDetailUpdateDTO
            {
                isValid = false,
            };
            if (id == null)
                return dto;
            TOrderDetail odd = _context.TOrderDetails
                .Include(od => od.productVariant)
                .ThenInclude(od => od.product)
                .FirstOrDefault(od => od.FProductVariantId == id);
            if (odd == null)
                return dto;
            dto.isValid = true;
            dto.OrderId = odd.FOrderId;
            dto.ProductVariantId = odd.FProductVariantId;
            dto.ProductName = odd.productVariant.product.FName;
            dto.UnitPrice = odd.FUnitPrice;
            dto.Quantity = odd.FQuantity;

            return (dto);
        }
    }
}
