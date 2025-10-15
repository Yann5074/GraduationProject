using ApiProject.Models;
using ApiProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers
{
    [Route("api/[Cart]")]
    [ApiController]
    public class CartController : Controller
    {
        private readonly CCartService _cartService;

        public CartController(CCartService cartService)
        {
            _cartService = cartService;
        }

        //列出購物車內容
        
    }
}
