using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using ApiProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // 列出購物車內容 -V
        //Get: api/Cart
        [HttpGet]
        public async Task<List<ResCartDTO>> GetAllCart()
        {
            var result = await _cartService.GetAllCartAsync();
            return result;
        }

        // 刪除購物車內某商品 -V
        //Delete: api/Cart/item/{cartItemId}
        [HttpDelete("item/{cartItemId}")]
        public async Task<IActionResult> DeleteCartItem(int cartItemId)
        {
            // #TODO 新增判斷 Cart.FMemberId 是否等於登入者
            if (cartItemId <= 0)
                return BadRequest();
            var result = await _cartService.DeleteCartItemAsync(cartItemId);
            if (!result.Ok)
            {
                if (result.Code < 500)
                    return StatusCode(result.Code, result);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }
            return Ok(result);
        }

        // 刪除購物車 -V
        // Delete: api/Cart/{cartId}
        [HttpDelete("{cartId}")]
        public async Task<IActionResult> DeleteCart(int cartId)
        {
            var result = await _cartService.DeleteCartAsync(cartId);
            if (!result.Ok)
            {
                if (result.Code < 500)
                    return StatusCode(result.Code, result);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }
            return NoContent();
        }

        // 編輯購物車物品數量 -V
        // Patch: api/Cart/item/{cartItemId}
        [HttpPatch("item/{cartItemId}")]
        public async Task<IActionResult> EditCartItemQty(int cartItemId, ReqEditCartItemNumDTO reqDto)
        {
            var result = await _cartService.EditCartItemNumAsync(cartItemId, reqDto);
            if (!result.Ok)
            {
                if (result.Code < 500)
                    return StatusCode(result.Code, result);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }
            return NoContent();
        }

        // 商品加入購物車 -V
        // Post: api/Cart/item
        [HttpPost("item")]
        public async Task<IActionResult> AddToCart([FromBody] ReqCartDTO reqDto)
        {
            var result = await _cartService.CreateCartAsync(reqDto);
            if (!result.Ok)
            {
                if (result.Code < 500)
                    return StatusCode(result.Code, result);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }
            return Ok(result);
        }

        //確認購物車是否正常 -V
        // Get: api/Cart/{memberId}
        [HttpGet("{memberId}")]
        public async Task<IActionResult> CheckCart(int memberId)
        {
            var result = await _cartService.ValidateCartAsync(memberId);
            if (!result.Ok)
            {
                if (result.Code < 500)
                    return StatusCode(result.Code, result);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }
            return NoContent();
        }
    }
}
