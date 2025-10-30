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

        public async Task<IActionResult> SendMessage(CancellationToken ct, [FromBody] ReqSendMessageDTO reqDto)  // ✅ 新增：後台員工身分)
        {
            var idCheck = await _memberAuth.ValidateAndGetMemberAsync(User, ct);
            TChatRoom room = await _context.TChatRooms
                .AsTracking()
                .FirstOrDefaultAsync(r => r.FChatRoomId == reqDto.chatRoomId);

            string senderId = idCheck.Member.FMemberId.ToString();
            string? SenderType = "member";



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
            TChatRoom currentRoom = await _context.TChatRooms
                .AsTracking()
                //撈出聊天室
                .FirstOrDefaultAsync(r => r.FChatRoomId == reqDto.chatRoomId);
            currentRoom.FLastMessageAt = now;
            _context.TChatRooms.Update(currentRoom);
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

        [HttpPost("InitVisitor")]
        public async Task<IActionResult> InitVisitor([FromBody] string visitorId)
        {
            if (string.IsNullOrWhiteSpace(visitorId))
                return BadRequest("visitorId 不能為空");

            // ✅ 檢查是否已存在聊天室
            var exist = await _context.TChatRooms
                .FirstOrDefaultAsync(c => c.FVisitorKey == visitorId);

            if (exist != null)
            {
                return Ok(new { chatRoomId = exist.FChatRoomId });
            }

            // ✅ 建立新聊天室
            var room = new TChatRoom
            {
                FVisitorKey = visitorId,
                FCreatedAt = DateTime.Now,
                FLastMessageAt = null
            };

            _context.TChatRooms.Add(room);
            await _context.SaveChangesAsync();

            return Ok(new { chatRoomId = room.FChatRoomId });
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

            return Ok(new { message = "已儲存表單", chatRoomId = form.FChatRoomId });
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
