using ApiProject.DTOs;
using ApiProject.Interfaces;
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
        [HttpGet]
        public async Task<List<ResOrderDTO>> GetAllOrders()
        {
            var order = await _orderService.GetAllOrdersAsync();
            return order;
        }

        // 找尋指定訂單 -V
        // GET:api/Order/keyword
        [HttpGet("{keyword}")]
        public async Task<List<ResOrderDTO>> GetOrdersByIdAndProdName(string? keyword)
        {
            if (keyword.IsNullOrEmpty())
                return null;
            var result = await _orderService.GetOrdersByIdAndProdNameAsync(keyword);
            return result;
        }

        // 刪除指定訂單 -V
        // DELETE:api/Order/{orderId}
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var result = await _orderService.DeleteOrderAsync(orderId);
            if (!result.Ok)
            {
                if (result.Code < 500)
                    return StatusCode(result.Code, result);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }
            return NoContent();
        }

        // 更改訂單地址 -V
        // Patch:api/Order/address/orderId
        [HttpPatch("address/{orderId}")]
        public async Task<IActionResult> EditDeliveryAddress(int orderId, ReqDeliveryAddressDTO reqDTO)
        {
            var result = await _orderService.EditDeliveryAddressAsync(orderId, reqDTO);
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
        // Patch:api/Order/taxno/{orderId}
        [HttpPatch("taxno/{orderId}")]
        public async Task<IActionResult> EditTaxNoAsync(int orderId, ReqTaxNoDTO reqDTO)
        {
            var result = await _orderService.EditTaxNoAsync(orderId, reqDTO);
            if (!result.Ok)
            {
                if (result.Code < 500)
                    return StatusCode(result.Code, result);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }
            return Ok(result);
        }

        // 會員新增訂單 -V
        // Post:api/Order
        [HttpPost("{memberId}")]
        public async Task<IActionResult> CreateOrderAsync(int memberId,[FromBody] ReqCreateOrderDTO reqDto)
        {
            //判斷購物車資料正確性
            var check = await _cartService.ValidateCartAsync(memberId);
            if (!check.Ok)
            {
                if (check.Code < 500)
                    return StatusCode(check.Code, check);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, "伺服器內部錯誤");
            }

            var result = await _orderService.CreateOrderFromCartAsync(memberId, reqDto);
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
        // Post:api/Order/guest



    }
}
