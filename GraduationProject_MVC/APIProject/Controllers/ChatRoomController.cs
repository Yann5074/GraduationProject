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

        // Controller Action
        [Authorize]
        [HttpPost("Index")]
        public async Task<IActionResult> Index(
            CancellationToken ct,
            [FromBody] ReqGetChartRoomDTO reqDto)
        {
            // 1) 取登入者（會員）
            var idCheck = await _memberAuth.ValidateAndGetMemberAsync(User, ct);
            if (idCheck?.Member == null) return Unauthorized();

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

            // 3) 會員端：只列出自己的聊天室（通常一間）
            //    🔑 這裡的 last 子查詢一定要綁 c.FChatRoomId（每間各查自己的最後訊息）
            var roomsQuery = await (
                from c in _context.TChatRooms
                where c.FMemberId == memberId
                join m in _context.TMembers on c.FMemberId equals m.FMemberId into gm
                from m in gm.DefaultIfEmpty()
                let last = _context.TMessages
                    .Where(x => x.FChatRoomId == c.FChatRoomId)                  // ✅ 關鍵：每個 c 自己的最後訊息
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
                .ToListAsync(ct);

            // 4)（可選）搜尋
            if (!string.IsNullOrWhiteSpace(reqDto.q))
            {
                var qTrim = reqDto.q.Trim();
                roomsQuery = roomsQuery
                    .Where(r => r.FName.Contains(qTrim) || r.FChatRoomId.ToString() == qTrim)
                    .ToList();
            }

            // 5) 決定選中的聊天室：前端若沒送，就用會員自己的
            int? selectedId = reqDto.chatRoomId ?? myRoom?.FChatRoomId;

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
                Rooms = roomsQuery.OrderByDescending(x => x.FLastMessageTime).ToList(),
                SelectedId = selectedId,
                Messages = messages
            };

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
