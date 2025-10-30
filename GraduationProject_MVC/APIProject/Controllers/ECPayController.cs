using ApiProject.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ECPayController : ControllerBase
    {
        private const string MerchantID = "3002607";
        private const string HashKey = "pwFHCqoQZGmho4w6";
        private const string HashIV = "EkRm7iFT261dpevs";

        // Post: api/ECPay/checkout
        [HttpPost("checkout")]
        //public IActionResult Checkout(ReqECPayDTO reqDTO)
        //{
        //    retutrn null;
        //}

    }
}
