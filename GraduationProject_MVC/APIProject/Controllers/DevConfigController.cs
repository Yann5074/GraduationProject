#if DEBUG

using Common.Notifications.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ApiProject.Controllers
{
    [ApiController]
    [Route("dev/config")]
    public class DevConfigController : ControllerBase
    {
        [HttpGet("mail")]
        public IActionResult GetMail(IOptionsSnapshot<EmailOptions> opt)
        {
            var o = opt.Value;
            return Ok(new
            {
                o.Host,
                o.Port,
                o.FromName,
                o.FromAddress,
                o.UserName,
                AppPassword = string.IsNullOrWhiteSpace(o.AppPassword) ? "(empty)" : "*****"
            });
        }
    }
}
#endif
