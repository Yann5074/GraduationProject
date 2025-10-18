using ApiProject.DTOs;
using ApiProject.Models;
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

        public async Task<IActionResult> CreateChatRoom(int? memberid, string? visitorid)
        {
            //判斷 memberid visitorid 不能同時為空   
            if (memberid == null && visitorid == null)
            {
                return BadRequest("Member ID and Visitor ID cannot both be null.");
            }
            //若是id 正確 就新增一筆資料到 TChatRoom
            //把一筆新的聊天室資料交給 EF Core， 讓它暫時放在 Change Tracker（追蹤集合）裡等待提交。
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
            //Entity ← 就是那個剛剛加入的 TChatRoom 物件
            //接著output ChatRoomid
            return Ok(ChatRoomid);
        }

        public async Task<IActionResult> sendmessage(int? chatRoomId, string message)
        {
            //檢查聊天室是否存在
            var room = await _context.TChatRooms
                .Include(c => c.member)
                .Include(c => c.Messages)
                .Where(c => c.FChatRoomId == chatRoomId)
                .Select(c => new ResChatRoomCreateDTO
                {
                    FChatRoomId = c.FChatRoomId,
                    image = c.Member != null && !string.IsNullOrEmpty(c.Member.FMemberImage)
                        ? c.Member.FMemberImage
                        : "https://i.imgur.com/8Km9tLL.jpg",
                    FVisitorId = c.FVisitorKey,
                    FName = c.Member != null && !string.IsNullOrEmpty(c.Member.FName)
                        ? c.Member.FName : "訪客",
                    FStatus = c.FStatus,
                    Fcontent = c.Messages
                        .OrderByDescending(m => m.FCreatedAt)
                        .Select(m => m.FContent)
                        .FirstOrDefault(),
                    FEmployeeId = c.FEmployeeId.ToString(),
                    FCreatedAt = c.FCreatedAt ?? DateTime.MinValue,
                    FClosedAt = c.FClosedAt
                })
                .FirstOrDefaultAsync();

            if (chatRoom == null)
            {
                return NotFound("Chat room not found.");
            }
            //在這裡你可以加入將訊息存入資料庫的邏輯
            //例如新增一個 TMessage 實體並設定相關屬性
            //然後將其加入到資料庫中
            //更新聊天室的最後訊息時間
            chatRoom.FLastMessageAt = DateTime.Now;

            var newMessage = new ResChatRoomCreateDTO
            {
                FChatRoomId = chatRoomId.Value,
                Fcontent = message,
                FCreatedAt = DateTime.Now

                
            };

            await _context.SaveChangesAsync();
            return Ok("Message sent successfully.");
        }

        public async Task<IActionResult> GetChatRooms(int? memberid, string? visitorid)
        {
            if (memberid == null && visitorid == null)
            {
                return BadRequest("Member ID and Visitor ID cannot both be null.");
            }
            var chatRooms = await 
            (from c in _context.TChatRooms 
             join m in _context.TMembers on c.FMemberId equals m.FMemberId into gm from m in gm.DefaultIfEmpty()
             let last = _context.TMessages
                 .Where(msg => msg.FChatRoomId == c.FChatRoomId)
                 .OrderByDescending(msg => msg.FCreatedAt)
                 .FirstOrDefault()
             where (memberid != null && c.FMemberId == memberid) || (visitorid != null && c.FVisitorKey == visitorid)
             orderby last.FCreatedAt descending
             select new  ResChatRoomCreateDTO
             {
                 FChatRoomId = c.FChatRoomId,
                 image = (m != null && !string.IsNullOrEmpty(m.FMemberImage)) ? m.FMemberImage : "https://i.imgur.com/8Km9tLL.jpg",
                 FVisitorId = c.FVisitorKey,
                 FName = (m != null && !string.IsNullOrEmpty(m.FName)) ? m.FName : "訪客",
                 FStatus = c.FStatus,
                 Fcontent = (last != null) ? last.FContent : "",
                 FEmployeeId = c.FEmployeeId.ToString(),
                 FCreatedAt = c.FCreatedAt ?? DateTime.MinValue,
                 FClosedAt = c.FClosedAt
             }
         
            ).ToListAsync();
            return Ok(chatRooms);
        }






        // GET: api/<ChatRoomController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ChatRoomController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ChatRoomController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ChatRoomController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ChatRoomController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
