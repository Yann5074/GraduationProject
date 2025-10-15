using GraduationProject.DTOs;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Controllers
{
    public class ChatRoomController : SuperController
    {
        //建構子示範直接把 DbContext 注入控制器
        dbFurniMartContext _context;
        public ChatRoomController(dbFurniMartContext context)
        {
            _context = context;
        }

        //每個聊天室 對應一個id

        //新增聊天室 
        //增加一筆聊天室id    firstmessage 訪客id 會員id   (本質是留言板) 拿會員或訪客 id創留言板 
        //input  memberid  visitorid  firstmessage
        //output ChatRoomid
        public IActionResult CreateChatRoom(int? memberid, string? visitorid)
        {
            //判斷 memberid visitorid 不能同時為空   
            if (memberid == null && visitorid == null)
            {
                return View("Error");
            }
            //若是id 正確 就新增一筆資料到 TChatRoom
            var a =  _context.TChatRooms.Add(new TChatRoom
            {
                FMemberId = memberid,
                FVisitorKey = visitorid,
                FFirstMessageAt = DateTime.Now,
                FLastMessageAt = DateTime.Now,
                FCreatedAt = DateTime.Now,
                FStatus = "open",
            });
            _context.SaveChanges();
            var ChatRoomid = a.Entity.FChatRoomId;
            //接著output ChatRoomid
            return View(ChatRoomid);
        }

        //聊天室 A 與 B singlR


        //查詢聊天室
        //p.s. 新增查詢功能 進階 可以用名稱 搜尋聊天室
        //input   memberid 
        //output  List<ChatRoomId>
        public IActionResult QueryChatRoom(int? id)
        {
            List<int> ChatRoomId = _context.TChatRooms
                .Where(c => c.FMemberId == id)
                .Select(c => c.FChatRoomId)
                .ToList();
            return View(ChatRoomId);
        }

        //刪除聊天室 (軟刪)  對應status  狀態為...關閉
        //input  ChatRoomid . memberid
        //output  
        public IActionResult DeleteChatRoom(int? id)
        {
            var chatRoom = _context.TChatRooms.FirstOrDefault(c => c.FChatRoomId == id);
            //判斷聊天室是否存在
            //如果存在 就狀態改為關閉
            if (chatRoom != null)
            {
                chatRoom.FStatus = "Close";
                chatRoom.FClosedAt = DateTime.Now;
                _context.SaveChanges();
            }
            //若不存在 就回傳錯誤
            else
            {
                return View("Error");
            }
            return View();
        }

        public async Task<IActionResult> Index(int? chatRoomId)
        {
            var rooms = await (
                from c in _context.TChatRooms
                join m in _context.TMembers on c.FMemberId equals m.FMemberId into gm
                from m in gm.DefaultIfEmpty() // ← left join
                let last = _context.TMessages
                    .Where(x => x.FChatRoomId == c.FChatRoomId)
                    .OrderByDescending(x => x.FCreatedAt)
                    .Select(x => new { x.FCreatedAt, x.FContent })
                    .FirstOrDefault()
                select new ChatRoomLlistDTO
                {
                    FChatRoomId = c.FChatRoomId,
                    image = (m != null && !string.IsNullOrEmpty(m.FMemberImage))
            ? m.FMemberImage
            : "/MemberHeadImages/default.png",

                    FName = (m != null && !string.IsNullOrEmpty(m.FName))
           ? m.FName
            : (c.FMemberId==null ? "訪客" : c.FMemberId.ToString()),

                    FLastMessageTime = last != null ? (DateTime?)last.FCreatedAt : null,
                    FLastMessage = last != null ? last.FContent : null
                })
                .OrderByDescending(x => x.FLastMessageTime)
                .ToListAsync();


            List<MessageDto> messages = new();
            if (chatRoomId.HasValue)
            {
                messages = await _context.TMessages
                    .Where(m => m.FChatRoomId == chatRoomId)
                    .OrderBy(m => m.FCreatedAt)
                    .Select(m => new MessageDto {
                    MessageId = m.FMessagesId,
                    SenderType = m.FSenderType,
                    SenderId = m.FSenderId,
                    Content = m.FContent,
                    CreatedAt = m.FCreatedAt
                    })
                    .ToListAsync();
            }

            var vm = new CChatRoomsPageVm { Rooms = rooms, SelectedId = chatRoomId, Messages = messages };
            return View(vm);
        }


        //public async Task<List<TMessage>> GetMessageList(int chatRoomId)
        //{
        //    var message = await _context.TMessages
        //        .Where(m => m.FChatRoomId == chatRoomId)
        //        .OrderBy(m => m.FCreatedAt)
        //        .ToListAsync();


        //    return message;
        //}
    }
}
