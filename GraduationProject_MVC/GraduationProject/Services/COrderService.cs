using GraduationProject.Interfaces;
using GraduationProject.ViewModels;
using GraduationProject.DTOs;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;

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
            string keywordMemberPhone = vm.txtKeywordMemberPhone;
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
                    MemberPhone = o.Member.FPhone,
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
            if (string.IsNullOrEmpty(keywordOrderId) && string.IsNullOrEmpty(keywordMemberName) && keywordMemberPhone != null)
                query = query.Where(o => o.MemberPhone.Contains(keywordMemberPhone));
            if (string.IsNullOrEmpty(keywordOrderId) && keywordMemberName != null && keywordMemberPhone != null)
                query = query.Where(o => o.MemberName.Contains(keywordMemberName) && o.MemberPhone.Contains(keywordMemberPhone));
            if (keywordOrderId != null && string.IsNullOrEmpty(keywordMemberName) && keywordMemberPhone != null)
                query = query.Where(o => o.OrderId.Contains(keywordOrderId) && o.MemberPhone.Contains(keywordMemberPhone));
            if (string.IsNullOrEmpty(keywordOrderId) && keywordMemberName != null && string.IsNullOrEmpty(keywordMemberPhone))
                query = query.Where(o => o.MemberName.Contains(keywordMemberName));
            if (keywordOrderId != null && keywordMemberName != null && string.IsNullOrEmpty(keywordMemberPhone))
                query = query.Where(o => o.OrderId.Contains(keywordOrderId) && o.MemberName.Contains(keywordMemberName));
            if (keywordOrderId != null && string.IsNullOrEmpty(keywordMemberName) && string.IsNullOrEmpty(keywordMemberPhone))
                query = query.Where(o => o.OrderId.Contains(keywordOrderId));
            if (keywordOrderId != null && keywordMemberName != null && keywordMemberPhone != null)
                query = query.Where(o => o.OrderId.Contains(keywordOrderId) && o.MemberName.Contains(keywordMemberName) && o.MemberPhone.Contains(keywordMemberPhone));
            return (query);
        }
        
        public void CreateOrder()
        {
            
        }
        public bool UpdateOrder(int? id)
        {
            if (id == null)
                return false;
            TOrder od = _context.TOrders.FirstOrDefault(o => o.FOrderId == id);
            if (od == null)
                return false;

        }

        public bool DeleteOrder(int? id)
        {
            if (id == null)
                return false;
            TOrder od = _context.TOrders.FirstOrDefault(o => o.FOrderId == id);
            if (od == null)
                return false;
            //_context.TOrders.Remove(od); 硬刪語法
            od.FOrderStatus = 6;
            _context.SaveChanges();
            return true;
        }
    }
}
