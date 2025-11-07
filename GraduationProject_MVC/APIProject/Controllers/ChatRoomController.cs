using ApiProject.DTOs;
using ApiProject.Hubs;
using ApiProject.Interfaces;
using ApiProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ApiProject.Models;



namespace ApiProject.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IAiProductService _svc;
        private readonly IHubContext<ChatHub> _hub;
        dbFurniMartContext _context;
        IHelpToolService _memberAuth;
        public ChatController(IAiProductService svc, dbFurniMartContext context, IHelpToolService memberAuth, IHubContext<ChatHub> hub)
        {
            _svc = svc;
            _hub = hub;
            _context = context;
            _memberAuth = memberAuth;
        }

        // 會員/使用者送出訊息（廣播原話）
        [HttpPost("UserSendBroadcast")]
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

 

        [Authorize]
        [HttpPost("Index")]
        public async Task<IActionResult> Index(
            CancellationToken ct)
        {
            // 1) 取登入者（會員）
            var idCheck = await _memberAuth.ValidateAndGetMemberAsync(User, ct);
            if (idCheck?.Member == null) return Unauthorized();


            //伏筆 使用者若有上萬人 每一個新聊天室就會佔用太多DB
            //var memberId = 15858;
            var memberId = idCheck.Member.FMemberId;

            // 2) 以會員ID找專屬聊天室（前端不需送 chatRoomId）
            var myRoom = await _context.TChatRooms
                .AsTracking()
                .FirstOrDefaultAsync(cr => cr.FMemberId == memberId, ct);

            // （可選）沒有就建立一間
            if (myRoom == null)
            {
                myRoom = new TChatRoom
                {
                    FMemberId = memberId,
                    FCreatedAt = DateTime.Now,
                    FLastMessageAt = null
                };
                _context.TChatRooms.Add(myRoom);
                await _context.SaveChangesAsync(ct);
            }





            // 5) 決定選中的聊天室：前端若沒送，就用會員自己的
            int? selectedId = myRoom?.FChatRoomId;

            // 6) 撈該聊天室訊息
            var messages = new List<ResMessageDto>();
            if (selectedId.HasValue)
            {
                messages = await _context.TMessages
                    .Where(m => m.FChatRoomId == selectedId.Value)
                    .OrderBy(m => m.FCreatedAt)
                    .Select(m => new ResMessageDto
                    {
                        MessageId = m.FMessagesId,
                        SenderType = m.FSenderType,
                        SenderId = m.FSenderId,
                        Content = m.FContent,
                        CreatedAt = m.FCreatedAt
                    })
                    .ToListAsync(ct);
            }

            // 7) 組回傳
            var vm = new ResChatRoomsPageDTO
            {
                Rooms = null,
                SelectedId = selectedId,
                Messages = messages
            };

            return Ok(vm);
        }
        // 觸發 AI 解析 + DB 篩選 → 推回 bot
        [HttpPost("TriggerAiResponseOnlyMember")]
        public async Task<IActionResult> Complete([FromBody] ReqCompleteChatDto req, CancellationToken ct)
        {
            try
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
            }
            catch (Exception ex)
            {
                // 記錄 log 後回 200 + bot 錯誤訊息，避免前端 Promise 抛 500
                await _hub.Clients.Group($"room:{req.ChatRoomId}")
                    .SendAsync("ReceiveMessage", new
                    {
                        chatRoomId = req.ChatRoomId,
                        senderType = "bot",
                        content = "抱歉，系統剛剛小當機了，我再幫您重找一次可以嗎？",
                        createdAt = DateTime.UtcNow
                    }, ct);
                return Ok(new { ok = false, error = "AI service failed." });
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
                });

            return Ok(result);
        }
    }
}
