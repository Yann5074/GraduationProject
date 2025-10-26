using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        public OrderController(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        // 列出所有訂單 -V
        // GET:api/Order
        [Authorize]
        [HttpGet]
        public async Task<List<ResOrderDTO>> GetAllOrders(CancellationToken ct)
        {
            var order = await _orderService.GetAllOrdersAsync(User, ct);
            return order;
        }

        // 找尋指定訂單 -V
        // GET:api/Order/keyword
        [Authorize]
        [HttpGet("{keyword}")]
        public async Task<List<ResOrderDTO>> GetOrdersByIdAndProdName(string? keyword, CancellationToken ct)
        {
            if (keyword.IsNullOrEmpty())
                return new List<ResOrderDTO>();
            var result = await _orderService.GetOrdersByIdAndProdNameAsync(keyword, User, ct);
            return result;
        }

        // 刪除指定訂單 -V
        // DELETE:api/Order/{orderId}
        [Authorize]
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId, CancellationToken ct)
        {
            var result = await _orderService.DeleteOrderAsync(orderId, User, ct);
            if (!result.Ok)
            {
                if (result.Code < 500)
                    return StatusCode(result.Code, result);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }
            return NoContent();
        }

        // 更改配送地址 -V
        // Patch:api/Order/address
        [Authorize]
        [HttpPatch("address")]
        public async Task<IActionResult> EditDeliveryAddress(ReqDeliveryAddressDTO reqDTO, CancellationToken ct)
        {
            var result = await _orderService.EditDeliveryAddressAsync(reqDTO, User, ct);
            if (!result.Ok)
            {
                if (result.Code < 500)
                    return StatusCode(result.Code, result);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }
            return Ok(result);
        }

        // 更改統編 -V
        // Patch:api/Order/taxno
        [Authorize]
        [HttpPatch("taxno")]
        public async Task<IActionResult> EditTaxNoAsync(ReqTaxNoDTO reqDTO, CancellationToken ct)
        {
            var result = await _orderService.EditTaxNoAsync(reqDTO, User, ct);
            if (!result.Ok)
            {
                if (result.Code < 500)
                    return StatusCode(result.Code, result);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }
            return Ok(result);
        }

        // 會員結帳 -V
        // Post:api/Order/CheckOut
        [Authorize]
        [HttpPost("CheckOut")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] ReqCreateOrderDTO reqDto, CancellationToken ct)
        {
            //判斷購物車資料正確性
            var check = await _cartService.ValidateCartAsync(User, ct);
            if (!check.Ok)
            {
                if (check.Code < 500)
                    return StatusCode(check.Code, check);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }

            var result = await _orderService.CreateOrderFromCartAsync(reqDto, User, ct);
            if (!result.Ok)
            {
                if (result.Code < 500)
                    return StatusCode(result.Code, result);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }
            return Ok(result);
        }

        // 訪客新增訂單
        // Post:api/Order/GuestCheckOut



    }
}
