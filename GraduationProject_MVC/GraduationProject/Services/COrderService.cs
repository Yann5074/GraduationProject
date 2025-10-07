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
        //列出/篩選訂單資訊
        public IEnumerable<OrderSearchDTO> SearchOrder(COrderSearchKeywordViewModel vm)
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
                .Select(o => new OrderSearchDTO
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
        
        //建立訂單
        public bool CreateOrder(OrderCreateDTO dtoUi)
        {
            var od = new TOrder
            {
                FMemberId = dtoUi.MemberId,
                FEmployeeId = dtoUi.EmployeeId,
                FTotalPrice = dtoUi.TotalPrice,
                FDiscount = dtoUi.Discount,
                FTaxNo = dtoUi.TaxNo,
                FOrderTime = dtoUi.OrderTime,
                FOrderStatus = dtoUi.OrderStatus,
                FPaymentMethod = dtoUi.PaymentMethod,
                FPaymentStatus = dtoUi.PaymentStatus,
                FPaymentTime = dtoUi.PaymentTime,
                FPickupMethod = dtoUi.PaymentMethod,
                FDeliveryStatus = dtoUi.DeliveryStatus,
                FDeliveryAddress = dtoUi.DeliveryAddress,
                FShippingCost = dtoUi.ShippingCost,
                FDeliveryTime = dtoUi.DeliveryTime,
                FLogisticsProvider = dtoUi.LogisticsProvider,
                FOrderCompletionTime = dtoUi.OrderCompletionTime,
                FNote = dtoUi.Note,
            };
            _context.TOrders.Add(od);
            _context.SaveChanges();
            return true;
        }

        //更新訂單
        public bool UpdateOrder(OrderUpdateDTO dtoUi)
        {
            var od = _context.TOrders.FirstOrDefault(o => o.FOrderId == dtoUi.OrderId);
            if (od == null)
                return false;
            od.FDiscount = dtoUi.Discount;
            od.FOrderStatus = dtoUi.OrderStatus;
            od.FPaymentStatus = dtoUi.PaymentStatus;
            od.FPickupMethod = dtoUi.PickupMethod;
            od.FDeliveryStatus = dtoUi.DeliveryStatus;
            od.FDeliveryAddress = dtoUi.DeliveryAddress;
            od.FShippingCost = dtoUi.ShippingCost;
            od.FDeliveryTime = dtoUi.DeliveryTime;
            od.FLogisticsProvider = dtoUi.LogisticsProvider;
            od.FOrderCompletionTime = dtoUi.OrderCompletionTime;
            od.FNote = dtoUi.FNote;
            _context.SaveChanges();

            return true;
        }

        //刪除訂單
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

        //尋找欲更新的訂單
        public OrderUpdateDTO SearchUpdateOrder(int? id)
        {
            var dto = new OrderUpdateDTO()
            {
                isValid = false
            };
            
            if (id == null)
                return dto;
            
            TOrder od = _context.TOrders
                .Include(o => o.Employee)
                .FirstOrDefault(o => o.FOrderId == id);
            
            if (od == null)
                return dto;

            dto.isValid = true;
            dto.OrderId = od.FOrderId;
            dto.EmployeeName = od.Employee?.FName ?? "--顧客網路下單--";
            dto.Discount = od.FDiscount;
            dto.OrderStatus = od.FOrderStatus;
            dto.PaymentStatus = od.FPaymentStatus;
            dto.PickupMethod = od.FPickupMethod;
            dto.DeliveryStatus = od.FDeliveryStatus;
            dto.DeliveryAddress = od.FDeliveryAddress;
            dto.ShippingCost = od.FShippingCost;
            dto.DeliveryTime= od.FDeliveryTime;
            dto.LogisticsProvider = od.FLogisticsProvider;
            dto.OrderCompletionTime = od.FOrderCompletionTime;
            dto.FNote = od.FNote;

            return dto;
        }

    }
}
