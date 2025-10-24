using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiProject.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class ChatRoomController : ControllerBase
    {
        dbFurniMartContext _context;
        IHelpToolService _memberAuth;
        public ChatRoomController(dbFurniMartContext context, IHelpToolService memberAuth)
        {
            _context = context;
            _memberAuth = memberAuth;
        }

        [Authorize]
        [HttpPost("Index")]
        public async Task<IActionResult> Index(CancellationToken ct,[FromBody] ReqGetChartRoomDTO reqDto)
        {
            var idCheck = await _memberAuth.ValidateAndGetMemberAsync(User, ct);
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

            if (!string.IsNullOrWhiteSpace(reqDto.q))
            {
                var qTrim = reqDto.q.Trim();
                roomsQuery = roomsQuery.Where(r =>
                    r.FName.Contains(qTrim) ||
                    r.FChatRoomId.ToString() == qTrim
                ).ToList(); // ← 記憶體篩選，用 ToList()
            }

            var rooms = roomsQuery
                .OrderByDescending(x => x.FLastMessageTime)
                .ToList(); // ← 記憶體排序，用 ToList()，不要 Async

            List<ResMessageDto> messages = new();
            if (reqDto.chatRoomId.HasValue)
            {
                messages = await _context.TMessages
                    .Where(m => m.FChatRoomId == reqDto.chatRoomId)
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

            var vm = new ResChatRoomsPageDTO { Rooms = rooms, SelectedId = reqDto.chatRoomId, Messages = messages };
            return Ok(vm);
        }

        /// <summary>
        /// 傳送訊息
        /// </summary>
        /// <param name="chatRoomId"></param>
        /// <param name="content"></param>
        /// <param name="visitorId"></param>
        /// <param name="memberId"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("SendMessage")]
        //[ValidateAntiForgeryToken]

        public async Task<IActionResult> SendMessage(CancellationToken ct,[FromBody] ReqSendMessageDTO reqDto)  // ✅ 新增：後台員工身分)
        {
            var idCheck = await _memberAuth.ValidateAndGetMemberAsync(User, ct);
            TChatRoom room = await _context.TChatRooms
                .AsTracking()
                .FirstOrDefaultAsync(r => r.FChatRoomId == reqDto.chatRoomId);

            string senderId = null;
            string? SenderType = null;

            var now = DateTime.Now;
            TMessage msg = new TMessage
            {
                FChatRoomId = reqDto.chatRoomId,
                FSenderId = senderId,   // ← string
                FContent = reqDto.content,
                FSenderType = SenderType,
                FCreatedAt = now
            };

            _context.TMessages.Add(msg);
            await _context.SaveChangesAsync();
            // 5) 更新聊天室最後訊息時間（若您表上有此欄位）

            room.FLastMessageAt = now;
            _context.TChatRooms.Update(room);
            await _context.SaveChangesAsync();

            //// 6) 回到 Index，維持目前聊天室與搜尋字  RedirectToAction跳轉到指定的動作方法
            //return RedirectToAction(nameof(Index), new { chatRoomId });
            return Ok(new
            {
                messageId = msg.FMessagesId,
                SenderType,
                senderId = senderId,
                createdAt = now
            });
        }


        [HttpPost("SubmitForm")]
        public async Task<IActionResult> SubmitForm([FromBody] TContactForm form)
        {
            if (string.IsNullOrEmpty(form.FContactName) ||
                string.IsNullOrEmpty(form.FCompanyName) ||
                string.IsNullOrEmpty(form.FPhone) ||
                string.IsNullOrEmpty(form.FEmail))
            {
                return BadRequest("表單欄位不能為空");
            }

            form.FCreatedAt = DateTime.Now;

            _context.TContactForms.Add(form);
            await _context.SaveChangesAsync();

            return Ok(new { message = "表單提交成功"});
        }

        //[HttpGet("Templates")]
        //public async Task<IActionResult> GetTemplates()
        //{
        //    var templates = await _context.TMessageTemplates
        //        .OrderBy(t => t.FTemplateId)
        //        .Select(t => new {
        //            title = t.FTitle,
        //            desc = t.FDescription,
        //            text = t.FContentText
        //        }).ToListAsync();

        //    return Ok(templates);
        //}




    }

}
