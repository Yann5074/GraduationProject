using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.ComponentModel;
using System.Security.Claims;
using Common.Notifications;
using Common.Notifications.Interfaces;

namespace ApiProject.Services
{
    public class COrderService:IOrderService
    {
        private readonly dbFurniMartContext _context;
        private readonly IHelpToolService _memberAuth;
        private readonly IOrderNotificationService _notify;
        private readonly ILogger<COrderService>? _logger;
        public COrderService(dbFurniMartContext context, IHelpToolService memberAuth, IOrderNotificationService notify, ILogger<COrderService>? logger = null)
        {
            _context = context;
            _memberAuth = memberAuth;
            _notify = notify;
            _logger = logger;
        }

        //列出訂單 -o
        public async Task<List<ResOrderDTO>> GetAllOrdersAsync(ClaimsPrincipal user, CancellationToken ct)
        {
            var idCheck = await _memberAuth.ValidateAndGetMemberAsync(user, ct);
            if (idCheck.Ok != true)
                return new List<ResOrderDTO>();
            var query = _context.TOrders
                .Include(o => o.Employee)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentStatus)
                .Include(o => o.PaymentMethod)
                .Include(o => o.DeliveryStatus)
                .Include(o => o.LogisticsProvider)
                .Where(o => o.FMemberId == idCheck.Member.FMemberId && o.FIsDeleted == 0) 
                .Select(o => new ResOrderDTO
                {
                    OrderId = o.FOrderId.ToString(),
                    //EmployeeId = o.FEmployeeId,
                    //EmployeeName = o.Employee == null? "未指定員工": o.Employee.FName,
                    EmployeeName = o.Employee.FName,
                    OrderTime = o.FOrderTime.ToString(),
                    OrderStatusId = o.FOrderStatus,
                    OrderStatus = o.OrderStatus.FStatusName,
                    TaxNo = o.FTaxNo,
                    PaymentMethod = o.PaymentMethod.FPaymentName,
                    PaymentStatus = o.PaymentStatus.FStatusName,
                    DeliveryAddress = o.FDeliveryAddress,
                    DeliveryStatus = o.DeliveryStatus.FDeliveryStatusName,
                    TotalPrice = o.FTotalPrice,
                    OrderDetail = o.OrderDetail
                    .Where(od => od.FIsDeleted == 0)
                    .Select(od => new ResOrderDetailDTO
                    {
                        ProductName = od.ProductVariant.Product.FName,
                        ProductInfo = ((int)od.ProductVariant.FLength).ToString() + " x " + ((int)od.ProductVariant.FWidth).ToString() + " x " + ((int)od.ProductVariant.FHeight).ToString() + " cm" +" / " + ((int)od.ProductVariant.FWeight).ToString() + " Kg",
                        UnitPrice = od.FUnitPrice,
                        Quantity = od.FQuantity,
                        ImageUrl = od.ProductVariant.ProductAsset.FUrl,
                    })
                });
            return await query.ToListAsync();
        }

        //搜尋訂單 -o
        public async Task<List<ResOrderDTO>> GetOrdersByIdAndProdNameAsync(string? keyword, ClaimsPrincipal user, CancellationToken ct)
        {
            var idCheck = await _memberAuth.ValidateAndGetMemberAsync(user, ct);
            if (idCheck.Ok != true)
                return new List<ResOrderDTO>();
            var query = _context.TOrders
                .Include(o => o.OrderDetail)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pro => pro.ProductAsset)
                .Include(o => o.OrderDetail)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pro => pro.Product)
                .Where(o => o.FMemberId == idCheck.Member.FMemberId && o.FIsDeleted == 0)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(o => o.FOrderId.ToString() == keyword || o.OrderDetail.Any(od => od.ProductVariant.Product.FName.Contains(keyword)));
            }

            var result = await query.Select(o => new ResOrderDTO
            {
                OrderId = o.FOrderId.ToString(),
                //EmployeeId = o.FEmployeeId,
                EmployeeName = o.Employee.FName,
                OrderTime = o.FOrderTime.ToString(),
                OrderStatusId = o.FOrderStatus,
                OrderStatus = o.OrderStatus.FStatusName,
                PaymentMethod = o.PaymentMethod.FPaymentName,
                PaymentStatus = o.PaymentStatus.FStatusName,
                TaxNo = o.FTaxNo,
                DeliveryAddress = o.FDeliveryAddress,
                DeliveryStatus = o.DeliveryStatus.FDeliveryStatusName,
                TotalPrice = o.FTotalPrice,
                OrderDetail = o.OrderDetail
                .Where(od => od.FIsDeleted == 0)
                .Select(od => new ResOrderDetailDTO
                {
                    ProductName = od.ProductVariant.Product.FName,
                    ProductInfo = od.ProductVariant.FLength.ToString() + " x" + od.ProductVariant.FWidth.ToString() + " x" + od.ProductVariant.FHeight.ToString() + " / " + od.ProductVariant.FWeight.ToString() + "Kg",
                    UnitPrice = od.FUnitPrice,
                    Quantity = od.FQuantity,
                    ImageUrl = od.ProductVariant.ProductAsset.FUrl,
                })

            }).OrderByDescending(o => o.OrderTime)
            .ToListAsync();

            return result;
        }

        // 刪除訂單 -o
        public async Task<ResultDTO> DeleteOrderAsync(int orderId, ClaimsPrincipal user, CancellationToken ct)
        {
            var idCheck = await _memberAuth.ValidateAndGetMemberAsync(user, ct);
            if (idCheck.Ok == false)
                return new ResultDTO
                {
                    Ok = idCheck.Ok,
                    Code = idCheck.Code,
                    Message = idCheck.Message,
                };
            TOrder order =  await _context.TOrders
                .Include(o => o.OrderDetail)
                .FirstOrDefaultAsync(o => o.FOrderId == orderId && o.FMemberId == idCheck.Member.FMemberId);
            if (order == null)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status404NotFound, //ASP.NET Core 常數
                    Message = "查無此訂單，請重新操作"
                };

            order.FIsDeleted = 1;
            // 連動刪除訂單明細
            var od = await _context.TOrderDetails.Where(od => od.FOrderId == order.FOrderId).ToListAsync();
            foreach (var item in od)
            {
                item.FIsDeleted = 1;
            }

            await _context.SaveChangesAsync();
            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status204NoContent,
            };
        }

        // 修改訂單配送地址 -o
        public async Task<ResultDTO> EditDeliveryAddressAsync(ReqDeliveryAddressDTO reqDTO, ClaimsPrincipal user, CancellationToken ct)
        {
            var idCheck = await _memberAuth.ValidateAndGetMemberAsync(user, ct);
            if (idCheck.Ok == false)
                return new ResultDTO
                {
                    Ok = idCheck.Ok,
                    Code = idCheck.Code,
                    Message = idCheck.Message,
                };
            var od = await _context.TOrders.FirstOrDefaultAsync(o => o.FOrderId == reqDTO.OrderId && o.FMemberId == idCheck.Member.FMemberId);
            if (od == null)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status404NotFound,
                    Message = "查無此訂單，請重新操作"
                };
            od.FDeliveryAddress = reqDTO.Address;
            await _context.SaveChangesAsync();
            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "配送地址更新完成"
            };
        }

        // 修改統編 -o
        public async Task<ResultDTO> EditTaxNoAsync(ReqTaxNoDTO reqDTO, ClaimsPrincipal user, CancellationToken ct)
        {
            var idCheck = await _memberAuth.ValidateAndGetMemberAsync(user, ct);
            if (idCheck.Ok == false)
                return new ResultDTO
                {
                    Ok = idCheck.Ok,
                    Code = idCheck.Code,
                    Message = idCheck.Message,
                };
            if (reqDTO.TaxNo == null)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "無效請求"
                };
            if (string.IsNullOrEmpty(reqDTO.TaxNo))
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "請輸入正確統編"
                };
            if (reqDTO.TaxNo.Length != 8 || !reqDTO.TaxNo.All(char.IsDigit))
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "統一編號須為8碼數字"
                };
            TOrder order = _context.TOrders.FirstOrDefault(o => o.FOrderId == reqDTO.OrderId && o.FMemberId == idCheck.Member.FMemberId);
            if (order == null)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status404NotFound,
                    Message = "查無此訂單，請重新操作"
                };
            order.FTaxNo = reqDTO.TaxNo;
            await _context.SaveChangesAsync();
            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "統編更新完成"

            };
        }

        // 從購物車建立訂單 -o
        public async Task<ResultDTO> CreateOrderFromCartAsync(ReqCreateOrderDTO reqDto, ClaimsPrincipal user, CancellationToken ct)
        {
            // 確認使用者登入狀態
            var idCheck = await _memberAuth.ValidateAndGetMemberAsync(user, ct);
            if (idCheck.Ok == false)
                return new ResultDTO
                {
                    Ok = idCheck.Ok,
                    Code = idCheck.Code,
                    Message = idCheck.Message,
                };

            // 確認是否為該會員購買
            if (reqDto.MemberId != idCheck.Member.FMemberId)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "訂單資訊錯誤，請重新操作"
                };

            // 判斷會員折扣
            var memLv = await _context.TLevels.FirstOrDefaultAsync(l => l.FLevelId == idCheck.Member.FLeveId);

            // 找出該用戶購物車
            var cart = await _context.TCarts
                .Include(c => c.CartItem.Where(ci => ci.FIsDeleted == 0))
                .FirstOrDefaultAsync(c => c.FMemberId == idCheck.Member.FMemberId && c.FIsDeleted == 0 && c.FIsCheckOut == 0);

            // 將購物車內容轉成訂單，先建立訂單在建立訂單明細

            // 建立訂單
            TOrder order = new TOrder
            {
                FIsDeleted = 0,
                FMemberId = idCheck.Member.FMemberId,
                FContactName = reqDto.ContactName,
                FContactPhone = reqDto.ContactPhone,
                FEmployeeId = reqDto.EmployeeId, // 畫面給予可填入ID的欄位
                FTotalPrice = Math.Round(cart.FTotalPrice * (decimal)memLv.FDiscount, 0, MidpointRounding.AwayFromZero) + (decimal)reqDto.ShippingCost,
                FDiscount = memLv.FDiscount,
                FTaxNo = reqDto.TaxNo,
                FOrderTime = DateTime.Now,
                FOrderStatus = 2, 
                FPaymentMethod = reqDto.PaymentMethod,
                FPaymentStatus = 1,
                //FPickupMethod = reqDto.PickupMethod,
                FPickupMethod = 2, //先寫死宅配，有機會再改成能改動的版本
                FDeliveryStatus = 1,
                FDeliveryAddress = reqDto.DeliveryAddress,
                FShippingCost = reqDto.ShippingCost,
                FLogisticsProvider = reqDto.LogisticsProvider,
                FNote = reqDto.Note
            };

            await _context.TOrders.AddAsync(order);
            await _context.SaveChangesAsync();

            // 建立訂單明細
            var orderDetail = new List<TOrderDetail>();
            foreach (var ci in cart.CartItem)
            {
                // 這邊未來也可插入折扣，優惠卷邏輯

                var od = new TOrderDetail
                {
                    FOrderId = order.FOrderId,
                    FProductVariantId = ci.FProductVariantId,
                    FIsDeleted = 0,
                    FUnitPrice = ci.FUnitPrice,
                    FQuantity = ci.FQuantity,
                };
                orderDetail.Add(od);
                //await _context.TOrderDetails.AddAsync(od); // 多次呼叫，效能不佳
            };

            await _context.TOrderDetails.AddRangeAsync(orderDetail);
            order.FTotalPrice = Math.Round(orderDetail.Sum(od => od.FQuantity * od.FUnitPrice) * (decimal)memLv.FDiscount) + (decimal)reqDto.ShippingCost;
            cart.FIsCheckOut = 1;

            // 扣除庫存
            var variantIds = orderDetail.Select(od => od.FProductVariantId)
                .Distinct()
                .ToList();

            var variants = await _context.TProductVariants
                .Where(v => variantIds.Contains(v.FProductVariantId))
                .ToDictionaryAsync(v => v.FProductVariantId, ct);

            foreach(var od in orderDetail)
            {
               if (!variants.TryGetValue(od.FProductVariantId, out var pv))
                {
                    return new ResultDTO
                    {
                        Ok = false,
                        Code = StatusCodes.Status400BadRequest,
                        Message = "查無此商品規格"
                    };
                }
                pv.FStock -= od.FQuantity;
            }

            await _context.SaveChangesAsync();

            // 建立信件需要的訂單明細資料 #TODO

            // 訂單建立完成，嘗試寄信
            try
            {
                var toEmail = idCheck.Member.FEmail;
                var customerName = idCheck.Member.FName;
                var total = order.FTotalPrice;

                await _notify.SendOrderCreatedAsync(
                    toEmail,
                    order.FOrderId,
                    customerName,
                    total,
                    ct
                    );
            }catch(Exception ex)
            {
                _logger?.LogError(ex, "SendOrderCreated mail failed. OrderId = {OrderId}", order.FOrderId);
            }

            return new ResultDTO
            { 
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "建立訂單成功",
                Data = new { orderId = order.FOrderId }

            };
        }

        // 從localstorage建立訂單 #TODO
        public async Task<ResultDTO> CreateOrderFromGuestAsync(ReqGuestOrderDTO reqDto)
        {
            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "訂單建立成功"
            };
        }
        
    }
}
