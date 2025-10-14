using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET:api/Order
        [HttpGet]
        public async Task<List<ResOrderDTO>> GetAllOrders()
        {
            var order = await _orderService.GetAllOrdersAsync();
            return order;
        }

        // GET:api/Order/keyword
        [HttpGet("{keyword}")]
        public async Task<List<ResOrderDTO>> GetOrdersByIdAndProdName(string? keyword)
        {
            if (keyword.IsNullOrEmpty())
                return null;
            var result = await _orderService.GetOrdersByIdAndProdNameAsync(keyword);
            return result;
        }

        // DELETE:api/Order/id
        [HttpDelete("id")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var result = await _orderService.DeleteOrderAsync(orderId);
            if (!result.Ok)
            {
                switch (result.Code)
                {
                    case StatusCodes.Status404NotFound:
                        return NotFound();
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            return NoContent();
        }

        // Patch:api/Order/id
        [HttpPatch("id")]
        public async Task<IActionResult> EditDeliveryAddress(int orderId, ReqDeliveryAddressDTO reqDTO)
        {
            var result = await _orderService.EditDeliveryAddressAsync(orderId, reqDTO);
            if (!result.Ok)
            {
                switch (result.Code)
                {
                    case StatusCodes.Status400BadRequest:
                        return BadRequest();
                    case StatusCodes.Status404NotFound:
                        return NotFound();
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            return Ok();
        }
    }
}
