using ApiProject.DTOs;
using ApiProject.Hubs;
using ApiProject.Interfaces;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using OpenAI.Chat;
using OpenAI.Chat;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ApiProject.Hubs
{
    public class ChatHub : Hub
    {


        dbFurniMartContext _context;
        IHubContext<ChatHub> _hub;
        IHelpToolService _memberAuth;
        private readonly ChatClient _chatClient;

        public ChatHub(dbFurniMartContext context, IHelpToolService memberAuth, IHubContext<ChatHub> hub, ChatClient chatClient)
        {
            //左 宣告變數  = 帶進來的值
            _context = context;
            _memberAuth = memberAuth;
            _hub = hub;
            _chatClient = chatClient;
        }


        public sealed class CompleteReqDto
        {
            public string ChatRoomId { get; set; }
            public string Message { get; set; } = "";
        }


        //[HttpPost]
//        public async Task CompleteChat( CompleteReqDto req)  //前端在呼叫 /complete 時一起送聊天室編號
//        {
//            var message = req.Message;
//            var extractPrompt = $@"
//只回 JSON（不要多餘文字），格式：
//{{""category"":""類別"",""color"":""顏色"",""price_min"":最小或null,""price_max"":最大或null}}
//使用者說：「{message}」";

//            try
//            {
//                //ChatCompletion completion = await _chatClient.CompleteChatAsync(extractPrompt);
//                //var json = completion.Content[0].Text;

//                //var parsed = JsonSerializer.Deserialize<QueryDto>(json);
//                //// Step 2️⃣：價格處理（像「一萬元左右」自動展開 ±20%） ps.寫在最下面
//                //(decimal? min, decimal? max) = ExpandApproxRangeIfNeeded(message, parsed.PriceMin, parsed.PriceMax);

//                ////Step 3️⃣：四表連查：Variant × Product × Category × Color
//                //// 先把條件拆好
//                //// 清理條件
//                //string cat = string.IsNullOrWhiteSpace(parsed.Category) ? "" : parsed.Category;
//                //string clr = string.IsNullOrWhiteSpace(parsed.Color) ? "" : parsed.Color;

//                decimal? min = 10000;
//                decimal? max = 12000;
//                string cat = "桌子";
//                string clr = "藍";
//                // ✅ 直接查完、直接變 DTO、

//                //// 1️⃣ 先查顏色
//                //var qColor = await _context.TColors
//                //    .Where(c => c.FColorName.Contains(clr))
//                //    .Select(c => c.FColorId)
//                //    .ToListAsync();
//                var colorQuery = _context.TColors.AsNoTracking();
//                if (!string.IsNullOrWhiteSpace(clr))
//                {
//                    colorQuery = colorQuery
//                        .Where(c => c.FColorName != null &&
//                                    EF.Functions.Like(c.FColorName, $"%{clr}%"));
//                }
//                var qColor = await colorQuery
//                    .Select(c => c.FColorId)
//                    .ToListAsync();   // List<int>

//                // 2) 分類
//                var catQuery = _context.TCategories.AsNoTracking();
//                if (!string.IsNullOrWhiteSpace(cat))
//                {
//                    catQuery = catQuery
//                        .Where(c => c.FName != null &&
//                                    EF.Functions.Like(c.FName, $"%{cat}%"));
//                }
//                var qCategory = await catQuery
//                    .Select(c => c.FCategoryId)
//                    .ToListAsync();   // List<int>

//                //3️⃣ 查產品（屬於這些分類）
//                var qProduct = await _context.TProducts
//                    .Where(p => qCategory.Contains(p.FCategoryId.Value))
//                    .Select(p => new { p.FProductId, p.FName })
//                    .ToListAsync();
//                // 4️⃣ 最後查變體（用顏色與產品 ID 篩選）
//                var productIds = qProduct.Select(p => (int?)p.FProductId).ToList();  // ✅ 轉成 List<int>

                
//                //// Step 1️⃣：先查變體
//                //var variantList = await _context.TProductVariants
//                //    .Where(v => qColor.Contains(v.FColorId.Value))
//                //    .Where(v => productIds.Contains(v.FProductId))
//                //    .Where(v => (!min.HasValue || v.FPrice >= min.Value) &&
//                //                (!max.HasValue || v.FPrice <= max.Value))
//                //    .OrderBy(v => v.FPrice)
//                //    .Take(5)
//                //    .ToListAsync();   // ✅ 先 ToListAsync()，資料回來再組 DTO

//                //// Step 2️⃣：在記憶體裡組 DTO
//                //var items = variantList.Select(v => new ProductPickDto
//                //{
//                //    ProductId = v.FProductId ?? 0,
//                //    Name = qProduct.FirstOrDefault(p => p.FProductId == v.FProductId)?.FName ?? "(未命名)",
//                //    Category = cat,
//                //    Color = clr,
//                //    Price = v.FPrice
//                //}).ToList();


//                //// 🟢 這裡直接推回聊天室（群組名請統一用 room:{chatRoomId}）
//                //var botText = (items?.Any() ?? false)
//                //    ? "找到幾個符合條件的商品，給您參考～"
//                //    : "目前沒有剛好符合的商品，我可以幫您換個條件再找找看喔～";

//                //await _hub.Clients              // 用注入進來的 IHubContext<ChatHub> 取得「所有連線的集合」
//                //    .Group($"room:{req.ChatRoomId}")            // 直接送到所有已透過 JoinRoom(chatRoomId) 加入 room{chatRoomId} 的連線；Hub 不必“接收”，因為 _hub 就是 Hub 的廣播器
//                //    .SendAsync("ReceiveMessage", new            // 發送 SignalR 事件給該群組的所有連線
//                //    {
//                //        chatRoomId = req.ChatRoomId,
//                //        senderType = "bot",
//                //        content = botText,
//                //        createdAt = DateTime.Now,
//                //        parsed = new
//                //        {
//                //            category = parsed.Category,
//                //            color = parsed.Color,
//                //            priceMin = min,
//                //            priceMax = max
//                //        },
//                //        items
//                //    });

//                // ✅ 仍回傳給呼叫端（選擇性使用）


//                // ... 後續處理
//            }
//            catch (System.ClientModel.ClientResultException ex)
//            {
//                // 大多數版本會有 Status / Message
//                var status = ex.Status;           // int? HTTP 狀態碼（若有）
//                var reason = ex.Message;          // SDK 組合的錯誤訊息

//                // 盡量把伺服器回的 raw body 印出來（不同版本屬性名稱可能不同，擇一可用就好）
                
//                // 記錄 & 回傳
//                Console.Error.WriteLine($"OpenAI Error {status}: {reason}\n");
//            }
          


//            //↓↓原本方法  取頭一筆資料
//            //return Ok(new { response = completion.Content[0].Text });
//        }




        public async Task CompleteChat(CompleteReqDto req)  // ✅ 改成 Task，移除 IActionResult
        {
            var message = req.Message;

            try
            {
                // 測試用的固定值
                decimal? min = 10000;
                decimal? max = 12000;
                string cat = "桌子";
                string clr = "藍";

                // ✅ 1️⃣ 查顏色 - 確保查詢完整執行
                var colorQuery = _context.TColors.AsNoTracking();
                if (!string.IsNullOrWhiteSpace(clr))
                {
                    colorQuery = colorQuery.Where(c => c.FColorName != null &&
                                                      EF.Functions.Like(c.FColorName, $"%{clr}%"));
                }
                var qColor = await colorQuery.Select(c => c.FColorId).ToListAsync();

                // ✅ 2️⃣ 查分類
                var catQuery = _context.TCategories.AsNoTracking();
                if (!string.IsNullOrWhiteSpace(cat))
                {
                    catQuery = catQuery.Where(c => c.FName != null &&
                                                   EF.Functions.Like(c.FName, $"%{cat}%"));
                }
                var qCategory = await catQuery.Select(c => c.FCategoryId).ToListAsync();

                // ✅ 3️⃣ 查產品
                var qProduct = await _context.TProducts
                    .Where(p => qCategory.Contains(p.FCategoryId.Value))
                    .Select(p => new { p.FProductId, p.FName })
                    .ToListAsync();

                var productIds = qProduct.Select(p => (int?)p.FProductId).ToList();

                // ✅ 4️⃣ 查變體
                var variantList = await _context.TProductVariants
                    .Where(v => qColor.Contains(v.FColorId.Value))
                    .Where(v => productIds.Contains(v.FProductId))
                    .Where(v => (!min.HasValue || v.FPrice >= min.Value) &&
                                (!max.HasValue || v.FPrice <= max.Value))
                    .OrderBy(v => v.FPrice)
                    .Take(5)
                    .ToListAsync();

                // ✅ 5️⃣ 組合 DTO
                var items = variantList.Select(v => new ProductPickDto
                {
                    ProductId = v.FProductId ?? 0,
                    Name = qProduct.FirstOrDefault(p => p.FProductId == v.FProductId)?.FName ?? "(未命名)",
                    Category = cat,
                    Color = clr,
                    Price = v.FPrice
                }).ToList();

                // ✅ 6️⃣ 推送訊息到聊天室
                var botText = items.Any()
                    ? "找到幾個符合條件的商品，給您參考～"
                    : "目前沒有剛好符合的商品，我可以幫您換個條件再找找看喔～";

                await Clients.Group($"room:{req.ChatRoomId}")
                    .SendAsync("ReceiveMessage", new
                    {
                        chatRoomId = req.ChatRoomId,
                        senderType = "bot",
                        content = botText,
                        createdAt = DateTime.Now,
                        parsed = new
                        {
                            category = cat,
                            color = clr,
                            priceMin = min,
                            priceMax = max
                        },
                        items
                    });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in CompleteChat: {ex.Message}");

                // 發送錯誤訊息給客戶端
                await Clients.Group($"room:{req.ChatRoomId}")
                    .SendAsync("ReceiveMessage", new
                    {
                        chatRoomId = req.ChatRoomId,
                        senderType = "bot",
                        content = "抱歉，查詢時發生錯誤，請稍後再試",
                        createdAt = DateTime.Now
                    });
            }
        }

        //--------







        public class QueryDto
        {
            [JsonPropertyName("category")]
            public string? Category { get; set; }
            [JsonPropertyName("color")]
            public string? Color { get; set; }
            //price_min 是 PriceMin。為了告訴它「這兩個是一樣的意思」
            [JsonPropertyName("price_min")]
            public decimal? PriceMin { get; set; }
            [JsonPropertyName("price_max")]
            public decimal? PriceMax { get; set; }
        }


        // 前端在進入聊天室後要先呼叫
        public async Task JoinRoom(string chatRoomId)
        {
            //room:16 門牌號碼
            var roomGroup = $"room:{chatRoomId}";

            //這條連線加入這個群組，之後 Clients.Group(roomGroup) 就能只推給在這組的連線。
            //Context.ConnectionId：這個使用者目前這條 SignalR 連線的 ID。
            await Groups.AddToGroupAsync(Context.ConnectionId, roomGroup);
        }

        // 如果要從 Hub 直接發送也可以用這支
        public async Task SendToRole(string chatRoomId, string senderType, string content)
        {
            if(senderType == "member")
            {
                CompleteReqDto sentoroleDTO = new CompleteReqDto {

                ChatRoomId = chatRoomId,
                Message = content

                };
                var Aifinal = CompleteChat(sentoroleDTO);
            }
            await Clients.Group($"room:{chatRoomId}").SendAsync("ReceiveMessage", content, senderType);
        }




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
