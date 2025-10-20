using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

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
                    TotalPrice = o.FTotalPrice,
                    OrderDetail = o.OrderDetail.Select(od => new ResOrderDetailDTO
                    {
                        ProductName = od.ProductVariant.Product.FName,
                        ProductInfo = od.ProductVariant.FLength.ToString() + " x" + od.ProductVariant.FWidth.ToString() + " x" + od.ProductVariant.FHeight.ToString() + " / " + od.ProductVariant.FWeight.ToString() + "Kg",
                        UnitPrice = od.FUnitPrice,
                        Quantity = od.FQuantity,
                        ImageUrl = od.ProductVariant.ProductAsset.FUrl,
                    })
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
                })

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
                    Code = StatusCodes.Status404NotFound, //ASP.NET Core 常數
                    Message = "查無此訂單，請重新操作"
                };
            //_context.TOrders.Remove(query); //硬刪寫法
            order.FIsDeleted = 1;
            await _context.SaveChangesAsync();
            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status204NoContent,
            };
        }

        // 修改訂單配送地址
        public async Task<ResultDTO> EditDeliveryAddressAsync(int orderId, ReqDeliveryAddressDTO reqDTO)
        {
            if (orderId != reqDTO.OrderId)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "訂單資訊錯誤，請重新操作"
                };
            var od = await _context.TOrders.FirstOrDefaultAsync(o => o.FOrderId == orderId);
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

        // 修改統編
        public async Task<ResultDTO> EditTaxNoAsync(int orderId, ReqTaxNoDTO reqDTO)
        {
            if (orderId != reqDTO.OrderId)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "訂單資訊錯誤，請重新操作"

                };
            TOrder order = _context.TOrders.FirstOrDefault(o => o.FOrderId == orderId);
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

        // 從購物車建立訂單
        public async Task<ResultDTO> CreateOrderFromCartAsync(int memberId, ReqCreateOrderDTO reqDto)
        {
            // 確認是否為該會員購買
            if (reqDto.MemberId != memberId)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "訂單資訊錯誤，請重新操作"
                };

            // 確認會員是否正常狀態
            var mem = await _context.TMembers
                .Include(c => c.Level)
                .FirstOrDefaultAsync(m => m.FMemberId == memberId && m.FStatus == 1);
            if (mem == null)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status404NotFound,
                    Message = "查無此會員或會員狀態異常，請洽客服"
                };

            // 找出該用戶購物車
            var cart = await _context.TCarts
                .Include(c => c.CartItem.Where(ci => ci.FIsDeleted == 0))
                .FirstOrDefaultAsync(c => c.FMemberId == memberId && c.FIsDeleted == 0 && c.FIsCheckOut == 0);
            if (IsCartEmpty(cart))
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "購物車為空，請加入您想購買的商品"
                };

            // 將購物車內容轉成訂單，先建立訂單在建立訂單明細

            // 建立訂單
            TOrder order = new TOrder
            {
                FIsDeleted = 0,
                FMemberId = memberId,
                FEmployeeId = reqDto.EmployeeId, // 畫面給予可填入ID的欄位
                FTotalPrice = cart.FTotalPrice,
                FDiscount = mem.Level.FDiscount,
                FTaxNo = reqDto.TaxNo,
                FOrderTime = DateTime.Now,
                FOrderStatus = 1,
                FPaymentMethod = reqDto.PaymentMethod,
                FPaymentStatus = 1,
                FPickupMethod = reqDto.PickupMethod,
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
            order.FTotalPrice = orderDetail.Sum(od => od.FQuantity * od.FUnitPrice);
            cart.FIsCheckOut = 1;

            await _context.SaveChangesAsync();

            return new ResultDTO
            { 
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "訂單建立成功"
            };
        }

        // 從localstorage建立訂單
        public async Task<ResultDTO> CreateOrderFromGuestAsync(ReqGuestOrderDTO reqDto)
        {
            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "訂單建立成功"
            };
        }

        // 確認購物車狀態 (內部邏輯)
        private bool  IsCartEmpty(TCart cart)
        {
            var result = (cart == null || cart.CartItem == null || !cart.CartItem.Any(ci => ci.FIsDeleted == 0));
            return result;
        }

        // 將購物車 (不論來源) 轉成訂單 (內部邏輯)
        private async Task CreateOrder(CartToOrderDTO dto)
        {
            var order = new TOrder
            {

            };

            var orderItem = new TOrderDetail
            {

            };
        }
    }
}
