using ApiProject.DTOs;
using ApiProject.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatRoomController : ControllerBase
    {
        dbFurniMartContext _context;
        public ChatRoomController(dbFurniMartContext context)
        {
            _context = context;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? q, int? chatRoomId)
        {
            var roomsQuery = await (
                from c in _context.TChatRooms
                join m in _context.TMembers on c.FMemberId equals m.FMemberId into gm
                from m in gm.DefaultIfEmpty() // ← left join
                let last = _context.TMessages
                    .Where(x => x.FChatRoomId == c.FChatRoomId)
                    .OrderByDescending(x => x.FCreatedAt)
                    .Select(x => new { x.FCreatedAt, x.FContent })
                    .FirstOrDefault()
                select new ResChatRoomListDTO
                {
                    FChatRoomId = c.FChatRoomId,
                    image = (m != null && !string.IsNullOrEmpty(m.FMemberImage))
            ? m.FMemberImage
            : "/MemberHeadImages/default.png",

                    FName = (m != null && !string.IsNullOrEmpty(m.FName))
           ? m.FName
            : (c.FMemberId == null ? "訪客" : c.FMemberId.ToString()),

                    FLastMessageTime = last != null ? (DateTime?)last.FCreatedAt : null,
                    FLastMessage = last != null ? last.FContent : null
                })
                .OrderByDescending(x => x.FLastMessageTime)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qTrim = q.Trim();
                roomsQuery = roomsQuery.Where(r =>
                    r.FName.Contains(qTrim) ||
                    r.FChatRoomId.ToString() == qTrim
                ).ToList(); // ← 記憶體篩選，用 ToList()
            }

            var rooms = roomsQuery
                .OrderByDescending(x => x.FLastMessageTime)
                .ToList(); // ← 記憶體排序，用 ToList()，不要 Async

            List<ResMessageDto> messages = new();
            if (chatRoomId.HasValue)
            {
                messages = await _context.TMessages
                    .Where(m => m.FChatRoomId == chatRoomId)
                    .OrderBy(m => m.FCreatedAt)
                    .Select(m => new ResMessageDto
                    {
                        MessageId = m.FMessagesId,
                        SenderType = m.FSenderType,
                        SenderId = m.FSenderId,
                        Content = m.FContent,
                        CreatedAt = m.FCreatedAt
                    })
                    .ToListAsync();
            }

            var vm = new ResChatRoomsPageDTO { Rooms = rooms, SelectedId = chatRoomId, Messages = messages };
            return Ok(vm);
        }

        [HttpPost("SendMessage")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int chatRoomId, string content)
        {
            TChatRoom room = await _context.TChatRooms
                .AsTracking()
                .FirstOrDefaultAsync(r => r.FChatRoomId == chatRoomId);
            string senderId = null;
            TMessage msg = new TMessage
            {
                FChatRoomId = chatRoomId,
                FSenderId = senderId,   // ← string
                FContent = content,
                FCreatedAt = DateTime.Now
            };

            _context.TMessages.Add(msg);
            await _context.SaveChangesAsync();
            // 5) 更新聊天室最後訊息時間（若您表上有此欄位）

            room.FLastMessageAt = DateTime.Now;
            _context.TChatRooms.Update(room);
            await _context.SaveChangesAsync();

            //// 6) 回到 Index，維持目前聊天室與搜尋字  RedirectToAction跳轉到指定的動作方法
            //return RedirectToAction(nameof(Index), new { chatRoomId });
            return Ok(new { redirectedTo = "Index", chatRoomId });
        }

       
        






    }

}
