using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Services
{
    public class COrderService:IOrderService
    {
        private readonly dbFurniMartContext _context;
        public COrderService(dbFurniMartContext context)
        {
            _context = context;
        }

        public async Task<List<ResOrderDTO>> GetAllOrdersAsync()
        {
            var query = _context.TOrders
                .Include(o => o.Employee)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentStatus)
                .Include(o => o.DeliveryStatus)
                .Include(o => o.LogisticsProvider)
                .Where(o => o.FIsDeleted != 1)
                .Select(o => new ResOrderDTO
                {
                    OrderId = o.FOrderId.ToString(),
                    EmployeeId = o.FEmployeeId,
                    EmployeeName = o.Employee.FName,
                    OrderTime = o.FOrderTime.ToString(),
                    OrderStatusId = o.FOrderStatus,
                    OrderStatus = o.OrderStatus.FStatusName,
                });
            return await query.ToListAsync();
        }

        public async Task<List<ResOrderDTO>> GetOrdersByIdAndProdNameAsync(string? keyword)
        {
            var query = _context.TOrders
                .Include(o => o.OrderDetail)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pro => pro.ProductAsset)
                .Include(o => o.OrderDetail)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pro => pro.Product)
                .Where(o => o.FIsDeleted != 1)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(o => o.FOrderId.ToString() == keyword || o.OrderDetail.Any(od => od.ProductVariant.Product.FName.Contains(keyword)));
            }

            var result = await query.Select(o => new ResOrderDTO
            {
                OrderId = o.FOrderId.ToString(),
                EmployeeId = o.FEmployeeId,
                EmployeeName = o.Employee.FName,
                OrderTime = o.FOrderTime.ToString(),
                OrderStatusId = o.FOrderStatus,
                OrderStatus = o.OrderStatus.FStatusName,
                TotalPrice = o.FTotalPrice,
                OrderDetail = o.OrderDetail.Select(od => new ResOrderDetailDTO
                {
                    ProductName = od.ProductVariant.Product.FName,
                    ProductInfo = od.ProductVariant.FLength.ToString() + " x" + od.ProductVariant.FWidth.ToString() + " x" + od.ProductVariant.FHeight.ToString() + " / " + od.ProductVariant.FWeight.ToString() + "Kg",
                    UnitPrice = od.FUnitPrice,
                    Quantity = od.FQuantity,
                    ImageUrl = od.ProductVariant.ProductAsset.FUrl,
                    
                }).ToList()

            }).OrderByDescending(o => o.OrderTime)
            .ToListAsync();

            return result;
        }
    }
}
