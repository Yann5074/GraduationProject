using ApiProject.DTOs;
using ApiProject.Hubs;
using ApiProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ApiProject.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IAiProductService _svc;
        private readonly IHubContext<ChatHub> _hub;

        public ChatController(IAiProductService svc, IHubContext<ChatHub> hub)
        {
            _svc = svc;
            _hub = hub;
        }

        // 會員/使用者送出訊息（廣播原話）
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ReqSendMessageDTO req, CancellationToken ct)
        {
            // 可在這裡寫 DB 訊息紀錄（略）
            await _hub.Clients.Group($"room:{req.ChatRoomId}")
                .SendAsync("ReceiveMessage", new
                {
                    chatRoomId = req.ChatRoomId,
                    senderType = req.SenderType, // member / user / employee
                    content = req.Content,
                    createdAt = DateTime.UtcNow
                }, ct);

            return Ok(new { ok = true });
        }

        // 觸發 AI 解析 + DB 篩選 → 推回 bot
        [HttpPost("complete")]
        public async Task<IActionResult> Complete([FromBody] ReqCompleteChatDto req, CancellationToken ct)
        {
            // 只有 user/member 走 AI；員工訊息直接廣播即可
            //↘↘如果 senderType 不是 "member"（不分大小寫）：→ 視為員工 / 其他身份，不做 AI。
            if (!string.Equals(req.SenderType, "member", StringComparison.OrdinalIgnoreCase)) 
            {
                await _hub.Clients.Group($"room:{req.ChatRoomId}")
                    .SendAsync("ReceiveMessage", new
                    {
                        chatRoomId = req.ChatRoomId,
                        senderType = req.SenderType,
                        content = req.Message,
                        createdAt = DateTime.UtcNow
                    }, ct);
                //不走 AI 的分支到此結束，回傳 200 OK（告知前端成功）。
                return Ok(new { ok = true });
            }

         // 下面走AI 解析 + DB 篩選 → 推回 bot
            var result = await _svc.ParseAndQueryAsync(req.Message, ct);

            await _hub.Clients.Group($"room:{req.ChatRoomId}")
                .SendAsync("ReceiveMessage", new
                {
                    chatRoomId = req.ChatRoomId,
                    senderType = "bot",
                    content = result.BotText,
                    createdAt = DateTime.UtcNow,
                    parsed = result.Parsed,
                    items = result.Items
                }, ct);

            return Ok(result);
        }
    }
}
