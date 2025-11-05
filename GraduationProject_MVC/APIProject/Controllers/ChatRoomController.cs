using ApiProject.DTOs;
using ApiProject.Hubs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using OpenAI.Chat;
using System.Drawing;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiProject.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ChatRoomController : ControllerBase
    {
        
        dbFurniMartContext _context;
        IHubContext<ChatHub> _hub;
        IHelpToolService _memberAuth;
        private readonly ChatClient _chatClient;

        public ChatRoomController(dbFurniMartContext context, IHelpToolService memberAuth,IHubContext<ChatHub> hub,ChatClient chatClient)
        {
            //左 宣告變數  = 帶進來的值
            _context = context;
            _memberAuth = memberAuth;
            _hub = hub;
            _chatClient = chatClient;
        }


        public sealed class CompleteReqDto
        {
            public int ChatRoomId { get; set; }
            public string Message { get; set; } = "";
        }


        [HttpPost("complete")]
        public async Task<IActionResult> CompleteChat([FromBody] CompleteReqDto req)  //前端在呼叫 /complete 時一起送聊天室編號
        {
            var message =  req.Message;
            var extractPrompt = $@"
只回 JSON（不要多餘文字），格式：
{{""category"":""類別"",""color"":""顏色"",""price_min"":最小或null,""price_max"":最大或null}}
使用者說：「{message}」";

            ChatCompletion completion = await 
        _chatClient.CompleteChatAsync(extractPrompt);

            var json = completion.Content[0].Text;

            var parsed = JsonSerializer.Deserialize<QueryDto>(json);

            // Step 2️⃣：價格處理（像「一萬元左右」自動展開 ±20%） ps.寫在最下面
        (decimal? min, decimal? max) = ExpandApproxRangeIfNeeded(message, parsed.PriceMin, parsed.PriceMax);

            //Step 3️⃣：四表連查：Variant × Product × Category × Color
        var query =
        from v in _context.TProductVariants.AsNoTracking()
        join p in _context.TProducts.AsNoTracking() on v.FProductId equals p.FProductId
        join cat in _context.TCategories.AsNoTracking() on p.FCategoryId equals cat.FCategoryId
        join col in _context.TColors.AsNoTracking() on v.FColorId equals col.FColorId
        select new                      // 投影成匿名物件，後面好用
        {
            VariantId = v.FProductVariantId,  // 變體主鍵
            ProductId = v.FProductId,         // 對應產品主鍵
            Name = p.FName,           // ✅ 產品名稱來自 tProduct
            Category = cat.FName,          // ✅ 類別名稱
            Color = col.FColorName,     // ✅ 顏色名稱
            Price = v.FPrice            // ✅ 價格
        };

            // 條件篩選
            if (!string.IsNullOrWhiteSpace(parsed.Category))    // 若 AI 有解析到類別
                query = query.Where(x => x.Category.Contains(parsed.Category!));   // 以包含做模糊查

            if (!string.IsNullOrWhiteSpace(parsed.Color))
                query = query.Where(x => x.Color.Contains(parsed.Color!));

            if (min.HasValue) query = query.Where(x => x.Price >= min.Value);
            if (max.HasValue) query = query.Where(x => x.Price <= max.Value);

            // 取前 5 筆（依價格排序）
            var items = await query
                .OrderBy(x => x.Price)
                .Take(5)                         //取五筆
                .Select(x => new ProductPickDto  // 投影成輸出用的 DTO  ps 該DTO寫在最底部
                {
                    ProductId = x.ProductId,
                    Name = x.Name,
                    Category = x.Category,
                    Color = x.Color,
                    Price = x.Price
                })
                .ToListAsync();               // 這裡才真正執行查詢，拿回 List

            // 🟢 這裡直接推回聊天室（群組名請統一用 room:{chatRoomId}）
            var botText = (items?.Any() ?? false)
                ? "找到幾個符合條件的商品，給您參考～"
                : "目前沒有剛好符合的商品，我可以幫您換個條件再找找看喔～";
             
            await _hub.Clients              // 用注入進來的 IHubContext<ChatHub> 取得「所有連線的集合」
                .Group($"room:{req.ChatRoomId}")            // 直接送到所有已透過 JoinRoom(chatRoomId) 加入 room{chatRoomId} 的連線；Hub 不必“接收”，因為 _hub 就是 Hub 的廣播器
                .SendAsync("ReceiveMessage", new            // 發送 SignalR 事件給該群組的所有連線
                {
                    chatRoomId = req.ChatRoomId,
                    senderType = "bot",
                    content = botText,
                    createdAt = DateTime.Now,
                    parsed = new
                    {
                        category = parsed.Category,
                        color = parsed.Color,
                        priceMin = min,
                        priceMax = max
                    },
                    items
                });

            // ✅ 仍回傳給呼叫端（選擇性使用）
            return Ok(new
            {
                parsed = new
                {
                    category = parsed.Category,
                    color = parsed.Color,
                    priceMin = min,
                    priceMax = max
                },
                items
            });

            //↓↓原本方法  取頭一筆資料
            //return Ok(new { response = completion.Content[0].Text });
        }

        public class QueryDto
        {
            public string? Category { get; set; }
            public string? Color { get; set; }
            //price_min 是 PriceMin。為了告訴它「這兩個是一樣的意思」
            [JsonPropertyName("price_min")]  
            public decimal? PriceMin { get; set; }
            [JsonPropertyName("price_max")]
            public decimal? PriceMax { get; set; }
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

            await _hub.Clients
             .Group($"room:{reqDto.chatRoomId}:member")
             .SendAsync("ReceiveMessage", new
             {
                 chatRoomId = reqDto.chatRoomId,
                 content = reqDto.content,
                 senderType = SenderType,
                 senderId = senderId,
                 createdAt = now
             }, ct);

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


        private class ProductPickDto
        {
            public int? ProductId { get; set; }
            public string Name { get; set; } = "";
            public string Category { get; set; } = "";
            public string Color { get; set; } = "";
            public decimal? Price { get; set; }
        }

        private static (decimal? min, decimal? max) ExpandApproxRangeIfNeeded(string text, decimal? min, decimal? max)
        {
            bool approx = text.Contains("左右") || text.Contains("大約") || text.Contains("約");

            if (min.HasValue && max.HasValue)
            {
                if (approx && min.Value == max.Value)
                {
                    var v = min.Value;
                    return (Math.Round(v * 0.8m), Math.Round(v * 1.2m));
                }
                return (min, max);
            }

            if (min.HasValue && !max.HasValue)
                return approx ? (min, Math.Round(min.Value * 1.2m)) : (min, min);

            if (!min.HasValue && max.HasValue)
                return approx ? (Math.Round(max.Value * 0.8m), max) : (max, max);

            // 若沒給價格，嘗試從句子中抓數字（例如「一萬」）
            var m = System.Text.RegularExpressions.Regex.Match(text, @"(\d{4,7})");
            if (m.Success && decimal.TryParse(m.Groups[1].Value, out var v2))
                return approx ? (Math.Round(v2 * 0.8m), Math.Round(v2 * 1.2m)) : (v2, v2);

            return (min, max);
        }


    }

}
