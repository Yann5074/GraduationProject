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

        public static class SenderTypes
        {
            public const string Employee = "employee";
            public const string Member = "member";
            public const string Visitor = "visitor";
            public const string Bot = "bot";
        }

        //每個聊天室 對應一個id

        //新增聊天室 
        //增加一筆聊天室id    firstmessage 訪客id 會員id   (本質是留言板) 拿會員或訪客 id創留言板 
        //input  memberid  visitorid  firstmessage
        //output ChatRoomid
        public async Task<IActionResult> CreateChatRoom(int? memberid, string? visitorid)
        {
            //判斷 memberid visitorid 不能同時為空   
            if (memberid == null && visitorid == null)
            {
                return View("Error");
            }
            //若是id 正確 就新增一筆資料到 TChatRoom
            var a = _context.TChatRooms.Add(new TChatRoom
            {
                FMemberId = memberid,
                FVisitorKey = visitorid,
                FFirstMessageAt = DateTime.Now,
                FLastMessageAt = DateTime.Now,
                FCreatedAt = DateTime.Now,
                FStatus = "open",
            });
            await _context.SaveChangesAsync();
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
                select new ChatRoomListDTO
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

            List<MessageDto> messages = new();
            if (chatRoomId.HasValue)
            {
                messages = await _context.TMessages
                    .Where(m => m.FChatRoomId == chatRoomId)
                    .OrderBy(m => m.FCreatedAt)
                    .Select(m => new MessageDto
                    {
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



        /// <summary>
        /// 做了兩件事 1. 新增一筆聊天室內容  2.更新最新的聊天室最後一筆訊息的時間
        /// </summary>
        /// <param name="chatRoomId">  前端畫面左側的ChatRoomList的選擇id </param>
        /// <param name="content">     前端畫面右側的messagelist 下面的 對話框裡面的 輸入內容 </param>
        /// <returns></returns>
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int chatRoomId, string content)
        {
            {
                TChatRoom room = await _context.TChatRooms
                    //AsTracking嘿～這些資料你要幫我記住它原本的樣子喔！」
                    .AsTracking()
                    .FirstOrDefaultAsync(r => r.FChatRoomId == chatRoomId);
                // senderid 用途 : 驗證身分  為何null  因為這裡少一個方法 
                // 少一個功能  去member裡拿出來的資料  伏筆:interface
                // 後台固定是員工
                var senderType = SenderTypes.Employee; // "employee"
                                                       // 1) 先試圖從登入者 Claims 取得員工編號
                var employeeIdFromClaims =
                    User?.FindFirst("EmployeeId")?.Value ??
                    User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                // 2) 取不到就用 1（依您的規則）
                var senderId = string.IsNullOrWhiteSpace(employeeIdFromClaims) ? "1000" : employeeIdFromClaims;


                //為何不加new會爆掉
                var msg = new TMessage
                {
                    FChatRoomId = chatRoomId,
                    FSenderType = senderType,
                    FSenderId = senderId,   // ← string
                    FContent = content,
                    FContentType = "text",
                    FCreatedAt = DateTime.Now
                };

                _context.TMessages.Add(msg);
                await _context.SaveChangesAsync();
                // 5) 更新聊天室最後訊息時間（若您表上有此欄位）
                room.FLastMessageAt = DateTime.Now;
                _context.TChatRooms.Update(room);
                await _context.SaveChangesAsync();
                // ✅ 直接回 JSON，不重整頁面

                return Json(new
                {
                    ok = true,
                    message = new
                    {
                        senderType = senderType,
                        content = content,
                        createdAt = msg.FCreatedAt?.ToString("yyyy/MM/dd HH:mm")
                    }
                });


                // 6) 回到 Index，維持目前聊天室與搜尋字  RedirectToAction跳轉到指定的動作方法
                // 為不重整頁面 註解下面這行
                //return RedirectToAction(nameof(Index), new { chatRoomId });
            }


        }
    }
}

