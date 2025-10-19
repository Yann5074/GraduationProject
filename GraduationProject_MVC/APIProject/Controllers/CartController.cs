using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using ApiProject.Services;
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

        // 列出購物車內容
        //Get: api/Cart
        [HttpGet]
        public async Task<List<ResCartDTO>> GetAllCart()
        {
            var result = await _cartService.GetAllCartAsync();
            return result;
        }

        // 刪除購物車內某商品
        //Delete: api/Cart/item/{cartItemId}
        [HttpDelete("item/{cartItemId}")]
        public async Task<IActionResult> DeleteCartItem(int cartItemId)
        {
            if (cartItemId <= 0)
                return BadRequest();
            var result = await _cartService.DeleteCartItemAsync(cartItemId);
            if (!result.Ok)
                return StatusCode(result.Code);
            return Ok();
        }

        // 刪除購物車
        // Delete: api/Cart/{cartId}
        [HttpDelete("{cartId}")]
        public async Task<IActionResult> DeleteCart(int cartId)
        {
            if (cartId <= 0)
                return BadRequest();
            var result = await _cartService.DeleteCartAsync(cartId);
            if (!result.Ok)
                return StatusCode(result.Code);
            return NoContent();
        }

        // 編輯購物車物品數量
        // Patch: api/Cart/item/{cartItemId}
        [HttpPatch("item/{cartItemId}")]
        public async Task<IActionResult> EditCartItemQty(int cartItemId, ReqEditCartItemNumDTO reqDto)
        {
            if (cartItemId <= 0 || cartItemId != reqDto.CartItemId)
                return BadRequest();
            var result = await _cartService.EditCartItemNumAsync(reqDto);
            if (!result.Ok)
                return StatusCode(result.Code);
            return Ok();
        }

        // 商品加入購物車
        // Post: api/Cart/item
        [HttpPost("item")]
        public async Task<IActionResult> AddToCart([FromBody] ReqCartDTO reqDto)
        {
            var result = await _cartService.CreateCartAsync(reqDto);
            if (!result.Ok)
                return StatusCode(result.Code);
            return Ok();
        }
    }
}
