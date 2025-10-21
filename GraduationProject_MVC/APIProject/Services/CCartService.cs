using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Diagnostics;

namespace ApiProject.Services
{
    public class CCartService : ICartService
    {
        private readonly dbFurniMartContext _context;
        public CCartService(dbFurniMartContext context)
        {
            _context = context;
        }

        // 列出購物車內容
        public async Task<List<ResCartDTO>> GetAllCartAsync()
        {
            var query = _context.TCarts
                .Include(c => c.CartItem)
                    .ThenInclude(c => c.ProductVariant)
                        .ThenInclude(c => c.Product)
                .Include(c => c.CartItem)
                    .ThenInclude(c => c.ProductVariant)
                        .ThenInclude(c => c.ProductAsset)
                .Where(c => c.FIsDeleted != 1 && c.FIsCheckOut != 1)
                .Select(c => new ResCartDTO
                {
                    MemberId = c.FMemberId,
                    TotalPrice = c.FTotalPrice,
                    CartItem = c.CartItem
                    .Where(ci => ci.FIsDeleted != 1)
                    .Select(ci => new ResCartItemDTO
                    {
                        CartItemId = ci.FCartItemId,
                        ProductVariantId = ci.FProductVariantId,
                        ProductName = ci.ProductVariant.Product.FName,
                        UnitPrice = ci.FUnitPrice,
                        Qty = ci.FQuantity,
                        SubTotal = ci.FSubtotal,
                        ImageUrl = ci.ProductVariant.ProductAsset.FUrl,
                        Size = $"{ci.ProductVariant.FLength} X {ci.ProductVariant.FWidth} X {ci.ProductVariant.FHeight} / {ci.ProductVariant.FWeight} Kg"
                    })
                });
            return await query.ToListAsync();
        }

        // 刪除購物車
        public async Task<ResultDTO> DeleteCartAsync(int cartId)
        {
            if (cartId <= 0)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "購物車資訊錯誤，請重新操作"
                };
            TCart cart = await _context.TCarts
                .FirstOrDefaultAsync(c => c.FCartId == cartId);
            if (cart == null)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status404NotFound,
                    Message = "查無此購物車，請重新操作"
                };
            cart.FIsDeleted = 1;
            var cartItem = await _context.TCartItems
                .Where(ci => ci.FCartId == cartId)
                .ToListAsync();
            foreach (var item in cartItem)
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

        // 編輯購物車物品數量
        public async Task<ResultDTO> EditCartItemNumAsync(int cartItemId, ReqEditCartItemNumDTO reqDto)
        {
            // 判斷Req購物車明細是否正確
            if (cartItemId <= 0 || cartItemId != reqDto.CartItemId)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "資料資訊錯誤，請重新確認"
                };

            // 判斷是否有該購物車明細
            var cartItem = await _context.TCartItems.FirstOrDefaultAsync(ci => ci.FCartItemId == reqDto.CartItemId);
            if (cartItem == null)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status404NotFound,
                    Message = "查無此資料，請重新操作"
                };

            // 判斷更改數量是否超過庫存或奇怪數字
            var pv = await _context.TProductVariants.FirstOrDefaultAsync(pv => pv.FProductVariantId == cartItem.FProductVariantId);
            if (reqDto.Qty > pv.FStock || reqDto.Qty < 0)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "商品數量錯誤或超過庫存，請重新操作"
                };
            cartItem.FQuantity = reqDto.Qty;
            await _context.SaveChangesAsync();
            await UpdatePriceAsync(cartItemId: reqDto.CartItemId);
            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status204NoContent
            };
        }

        // 刪除購物車內某商品
        public async Task<ResultDTO> DeleteCartItemAsync(int cartItemId)
        {

            TCartItem item = await _context.TCartItems.FirstOrDefaultAsync(i => i.FCartItemId == cartItemId);
            if (item == null)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status404NotFound,
                    Message = "查無此商品，請重新操作"
                };
            item.FIsDeleted = 1;
            await _context.SaveChangesAsync();
            await UpdatePriceAsync(cartItemId: cartItemId);
            await CheckCartEmptyAsync(item.FCartId);
            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "商品刪除成功"
            };
        }

        // 商品加入購物車
        public async Task<ResultDTO> CreateCartAsync(ReqCartDTO reqDto)
        {
            // 確認是否有該商品
            var prod = await _context.TProductVariants.FirstOrDefaultAsync(p => p.FProductVariantId == reqDto.ProductVariantId && p.FPstatus == 1);
            if (prod == null)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status404NotFound,
                    Message = "查無此商品，請重新操作"
                };
            // 確認商品數量正確
            if (prod.FStock <= reqDto.Qty || reqDto.Qty <= 0)
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "商品數量錯誤，請重新操作"
                };

            //確認是否有購物車
            TCart query = await _context.TCarts
                .Include(c => c.CartItem)
                .FirstOrDefaultAsync(c => c.FMemberId == reqDto.MemberId && c.FIsDeleted == 0 && c.FIsCheckOut == 0);
            if (query == null)
            {
                query = new TCart
                {
                    FDate = DateTime.Now.ToString(),
                    FIsCheckOut = 0,
                    FIsDeleted = 0,
                    FTotalPrice = 0,
                    FMemberId = reqDto.MemberId,
                };
                _context.TCarts.Add(query);
                await _context.SaveChangesAsync(); // 先進行儲存以產生CartId
            }

            //判斷是否有該物品
            TCartItem cartItem = query.CartItem.FirstOrDefault(ci => ci.FProductVariantId == reqDto.ProductVariantId);
            if (cartItem != null) //內有紀錄該產品
            {
                if (cartItem.FIsDeleted == 0)
                {
                    cartItem.FQuantity += reqDto.Qty;
                    // 更換商品的價格為最新價格
                    cartItem.FUnitPrice = (decimal)prod.FPrice;
                    cartItem.FSubtotal = cartItem.FQuantity * cartItem.FUnitPrice;
                }
                if (cartItem.FIsDeleted == 1)
                {
                    cartItem.FIsDeleted = 0;
                    cartItem.FQuantity = reqDto.Qty;
                    cartItem.FUnitPrice = (decimal)prod.FPrice;
                    cartItem.FSubtotal = cartItem.FUnitPrice * cartItem.FQuantity;
                }
                await _context.SaveChangesAsync();
            }
            //沒有紀錄該產品
            if (cartItem == null)
            {
                cartItem = new TCartItem
                {
                    FCartId = query.FCartId,
                    FIsDeleted = 0,
                    FProductVariantId = reqDto.ProductVariantId,
                    FQuantity = reqDto.Qty,
                    FUnitPrice = (decimal)prod.FPrice,
                    FSubtotal = reqDto.Qty * (decimal)prod.FPrice
                };
                await _context.TCartItems.AddAsync(cartItem);
            }
            await _context.SaveChangesAsync();

            await UpdatePriceAsync(cartItemId: cartItem.FCartItemId);


            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "商品成功加入購物車"
            };
        }

        // 進入購物車頁面時，確認購物車內容正確性
        public async Task<ResultDTO> ValidateCartAsync(int memberId)
        {
            // 找出該用戶購物車
            var cart = await _context.TCarts
                .Include(c => c.CartItem.Where(ci => ci.FIsDeleted == 0))
                    .ThenInclude(c => c.ProductVariant)
                        .ThenInclude(c => c.Product)
                .FirstOrDefaultAsync(c => c.FMemberId == memberId && c.FIsDeleted == 0 && c.FIsCheckOut == 0);

            if (IsCartEmpty(cart))
                return null;

            // 確認該商品狀態與數量正常
            var invalidItem = new List<string>();
            foreach (var item in cart.CartItem)
            {
                var pro = await _context.TProductVariants
                    .Include(p => p.Product)
                    .FirstOrDefaultAsync(p => p.FProductVariantId == item.FProductVariantId && p.FPstatus == 1);
                // 確認商品是否存在
                if (pro == null)
                {
                    invalidItem.Add($"商品 {item.ProductVariant.Product.FName} 不存在，請重新操作");
                    continue;
                }
                // 確認商品數量正常
                if (item.FQuantity >= pro.FStock || item.FQuantity <= 0)
                {
                    invalidItem.Add($"商品 {item.ProductVariant.Product.FName} 數量異常，請重新操作");
                    continue;
                }

                //確認商品價格正常
                if (item.FUnitPrice != pro.FPrice)
                {
                    invalidItem.Add($"商品 {item.ProductVariant.Product.FName} 價格異常，請重新操作購物車");
                }
            }
            if (invalidItem.Any())
            {
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = invalidItem
                };
            }

            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status204NoContent,
            };
        }

        // 確認購物車狀態 (內部邏輯)
        private bool IsCartEmpty(TCart cart)
        {
            var result = (cart == null || cart.CartItem == null || !cart.CartItem.Any(ci => ci.FIsDeleted == 0)); //若購物車不完整存在，回傳true
            return result;
        }

        // 登入後將 pinia 購物車轉入SQL 


        // 計算購物車金額 (內部邏輯)
        private async Task UpdatePriceAsync(int? cartItemId = null, int? cartId = null)
        {
            // Subtotal 計算
            if (cartItemId.HasValue)
            {
                var query = await _context.TCartItems.FirstOrDefaultAsync(ci => ci.FCartItemId == cartItemId);
                if (query == null)
                    return;
                query.FSubtotal = query.FUnitPrice * query.FQuantity;
                await _context.SaveChangesAsync();
                cartId = query.FCartId;
            }

            // TotalPrice 計算
            if (cartId.HasValue)
            {
                var query = await _context.TCarts
                .Include(c => c.CartItem)
                .FirstOrDefaultAsync(c => c.FCartId == cartId);
                if (query == null)
                    return;
                query.FTotalPrice = query.CartItem
                    .Where(ci => ci.FIsDeleted == 0)
                    .Sum(ci => ci.FSubtotal);

                await _context.SaveChangesAsync();
            }

        }

        // CartItem為空則Cart刪除 (內部邏輯)
        private async Task CheckCartEmptyAsync(int cartId)
        {
            var check = await _context.TCartItems
                .AnyAsync(ci => ci.FCartId == cartId && ci.FIsDeleted == 0); // 若還有CartItem 便回覆 true
            if (!check)
            {
                var cart = await _context.TCarts.FindAsync(cartId);
                cart.FIsDeleted = 1;
                await _context.SaveChangesAsync();
            }
        }


    }
}
