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
        //列出訂單
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
                    //EmployeeName = o.Employee == null? "未指定員工": o.Employee.FName,
                    EmployeeName = o.Employee.FName,
                    OrderTime = o.FOrderTime.ToString(),
                    OrderStatusId = o.FOrderStatus,
                    OrderStatus = o.OrderStatus.FStatusName,
                });
            return await query.ToListAsync();
        }

        //搜尋訂單
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

        // 刪除訂單
        public async Task<ResultDTO> DeleteOrderAsync(int orderId)
        {
            TOrder order =  await _context.TOrders.FirstOrDefaultAsync(o => o.FOrderId == orderId);
            if (order == null)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status404NotFound //ASP.NET Core 常數
                };
            //_context.TOrders.Remove(query); //硬刪寫法
            order.FIsDeleted = 1;
            await _context.SaveChangesAsync();
            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status204NoContent
            };
        }
    }
}
