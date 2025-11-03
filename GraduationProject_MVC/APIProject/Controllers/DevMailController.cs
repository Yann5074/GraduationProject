#if DEBUG

using Common.Notifications.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers
{
    [ApiController]
    [Route("dev/mail")]
    public class DevMailController : ControllerBase
    {
        public record PingRequest(string? To);

        [HttpPost("ping")]
        public async Task<IActionResult> PingAsync([FromServices] IEmailSender sender, [FromBody] PingRequest? body, [FromQuery] string? to, CancellationToken ct = default)
        {
            var target = body?.To ?? to ?? "lby110070@gmail.com";
            try
            {
                await sender.SendHtmlAsync(target, "測試- 通知服務線路OK", "<p>路線OK</p>", ct: ct);
                return Ok(new { ok = true, to = target });
            }
            catch (Exception ex)
            {
                return Problem(title: "SMTP失敗", detail: ex.Message, statusCode: 500);
            }
        }
    }
}
#endif
