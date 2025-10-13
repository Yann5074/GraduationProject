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
                .Include(o => o.Member)
                .Include(o => o.Employee)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentStatus)
                .Include(o => o.DeliveryStatus)
                .Include(o => o.LogisticsProvider)
                .Where(o => o.FIsDeleted != 1)
                .Select(o => new ResOrderDTO
                {
                    OrderId = o.FOrderId.ToString(),
                    MemberName = o.Member.FName,
                    EmployeeId = o.FEmployeeId,
                    EmployeeName = o.Employee.FName,
                    OrderTime = o.FOrderTime.ToString(),
                    OrderStatusId = o.FOrderStatus,
                    OrderStatus = o.OrderStatus.FStatusName,
                    PaymentStatusId = o.FPaymentStatus,
                    PaymentStatus = o.PaymentStatus.FStatusName,
                    DeliveryStatusId = o.FDeliveryStatus,
                    DeliveryStatus = o.DeliveryStatus.FDeliveryStatusName
                });
            return await query.ToListAsync();
        }

        public async Task<List<ResOrderDTO>> GetOrdersByIdAndProdNameAsync(string? keyword)
        {
            var query = _context.TOrders
                .Include(o => o.OrderDetail)
                .ThenInclude(od => od.FProductVariantId)
                .ThenInclude(pro => pro.)
        }
    }
}
