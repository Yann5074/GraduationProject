using GraduationProject.Interfaces;
using GraduationProject.ViewModels;
using GraduationProject.DTOs;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    public class COrderService:IOrderService
    {
        //注入語法
        private readonly dbFurniMartContext _context;
        public COrderService(dbFurniMartContext context)
        {
            _context = context;
        }

        //Interface實作
        public IEnumerable<OrderDTO> SearchOrder(COrderSearchKeywordViewModel vm)
        {
            string keywordOrderId = vm.txtKeywordOrderId;
            string keywordMemberName = vm.txtKeywordMemberName;
            string keywordMemberPhone = vm.txtKeywordMemverPhone;
            var query = _context.TOrders
                .Include(o => o.Member)
                .Include(o => o.Employee)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentStatus)
                .Include(o => o.DeliveryStatus)
                .Include(o => o.LogisticsProvider)
                .Select(o => new OrderDTO
                {
                    OrderId = o.FOrderId.ToString(),
                    MemberName = o.Member.FName,
                    EmployeeName = o.Employee.FName,
                    OrderTime = o.FOrderTime.ToString(),
                    OrderStatus = o.OrderStatus.FStatusName,
                    PaymentStatus = o.PaymentStatus.FStatusName,
                    DeliveryStatus = o.DeliveryStatus.FDeliveryStatusName,
                    LogisticsProvider = o.LogisticsProvider.FLogisticsProviderName,
                    Note = o.FNote
                });
            if (string.IsNullOrEmpty(keywordOrderId) && string.IsNullOrEmpty(keywordMemberName) && string.IsNullOrEmpty(keywordMemberPhone))
                //query = query.Where(o => o.OrderStatus != "訂單取消");
                return (query);
            return (query);
        }
        
        public void CreateOrder()
        {
            
        }
        public void UpdateOrder()
        {

        }
        public void DeleteOrder()
        {

        }
    }
}
