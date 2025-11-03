using GraduationProject.Interfaces;
using GraduationProject.ViewModels;
using GraduationProject.DTOs;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using Common.Notifications;
using Common.Notifications.Interfaces;

namespace GraduationProject.Services
{
    public class COrderService:IOrderService
    {
        //注入語法
        private readonly dbFurniMartContext _context;
        private readonly IOrderNotificationService _notify;
        private readonly ILogger<COrderService>? _logger; 
        public COrderService(dbFurniMartContext context, IOrderNotificationService notify, ILogger<COrderService>? logger)
        {
            _context = context;
            _notify = notify;
            _logger = logger;
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
                    IsDeleted = o.FIsDeleted,
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
            // 不顯示已刪除的訂單
            query = query.Where(o => o.IsDeleted != 1);
            if (string.IsNullOrEmpty(keywordOrderId) && string.IsNullOrEmpty(keywordMemberName) && string.IsNullOrEmpty(keywordMemberPhone))
                return query;
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
                FIsDeleted = 0,
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

        //更新訂單 (改為非同步版本)
        public async Task<bool> UpdateOrder(OrderUpdateDTO dtoUi, CancellationToken ct = default)
        {
            var od = await _context.TOrders
                .Include(o => o.Member)
                .FirstOrDefaultAsync(o => o.FOrderId == dtoUi.OrderId);
            if (od == null)
                return false;

            var oldOrderStatus = od.FOrderStatus; // 舊訂單狀態
            var oldOrderDelivery = od.FDeliveryStatus; //舊運送狀態
            var customer = await _context.TMembers
                .FirstOrDefaultAsync(c => c.FMemberId == od.FMemberId);

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

            if (od.FDeliveryStatus == 4)
            {
                od.FOrderStatus = 5;
            }

            if (od.FOrderStatus == 5 && od.FPaymentStatus == 4)
            {
                od.FOrderCompletionTime = DateTime.Now;
                customer.FMoneySum += (int)(od.FTotalPrice - od.FShippingCost);
            }

            var newLevelId = await _context.TLevels
                .Where(l => customer.FMoneySum >= l.FUpgradeRules)
                .OrderByDescending(l => l.FUpgradeRules)
                .Select(l => l.FLevelId)
                .FirstOrDefaultAsync();

            if (customer.FLeveId != newLevelId)
            {
                customer.FLeveId = newLevelId;
            }

            await _context.SaveChangesAsync();

            try
            {
                var toEmail = od.Member?.FEmail;
                var name = od.Member?.FName;
                if (!string.IsNullOrWhiteSpace(toEmail) && !string.IsNullOrWhiteSpace(name))
                {
                    if (oldOrderStatus != od.FOrderStatus)
                    {
                        await _notify.SendOrderStatusChangedAsync(
                            toEmail!,
                            od.FOrderId,
                            name!,
                            MapOrderStatus(od.FOrderStatus),
                            ct
                            );
                    }

                    if (oldOrderDelivery != od.FDeliveryStatus)
                    {
                        await _notify.SendOrderDeliveryChangedAsync(
                            toEmail!,
                            od.FOrderId,
                            name!,
                            MapOrderDelivery(od.FDeliveryStatus),
                            ct
                            );
                    }
                }

                return true;
            }catch(DbUpdateException ex )
            {
                return false;
            }catch(Exception ex)
            {
                return false;
            }

        }

        // 內部對應 - 訂單狀態
        private static string MapOrderStatus(int code) => code switch
        {
            1 => "處理中",
            2 => "訂單成立",
            3 => "付款資訊確認",
            4 => "訂單出貨",
            5 => "訂單完成"
        };

        //內部對應 - 運送狀態
        private static string MapOrderDelivery(int code) => code switch
        {
            1 => "備貨中",
            2 => "運送中",
            3 => "已送達",
            4 => "已取貨",
            5 => "退貨中",
            6 => "已退貨"
        };


        //刪除訂單
        public bool DeleteOrder(int? id)
        {
            if (id == null)
                return false;
            TOrder od = _context.TOrders.FirstOrDefault(o => o.FOrderId == id);
            if (od == null)
                return false;
            //_context.TOrders.Remove(od); 硬刪語法
            od.FIsDeleted = 1; //軟刪 
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
            dto.EmployeeName = od.Employee?.FName ?? "--網路下單--";
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
