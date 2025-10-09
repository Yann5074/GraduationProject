using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class ChatRoomController : Controller
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
                FStatus = "open"
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
                chatRoom.FStatus = "closed";
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

        public IActionResult Index()
        {
            //我要輸入的employeeid 來查詢所有開啟的聊天室 P.S.這邊要判斷是否為會員

            //output 聊天室的 chatroomid 以及 會員id 或 訪客id 訊息 建立時間 與 關閉時間
            _context.TChatRooms
                
                .Select(c => new ChatRoomViewModel
                {
                    c.FChatRoomId,
                    c.FMemberId,
                    c.FVisitorKey,
                    c.FCreatedAt,
                    c.FClosedAt
                })
                .ToList();

            return View();
        }
    }
}
