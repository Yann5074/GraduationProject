using ApiProject.DTOs;
using ApiProject.DTOs.Responses;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiProject.Services
{
    public sealed class AiProductService : IAiProductService
    {
        private readonly dbFurniMartContext _db;
        private readonly ChatClient _chat;
        //chat client 來自 OpenAI SDK
        public AiProductService(dbFurniMartContext db, ChatClient chat)
        {
            _db = db;
            _chat = chat;
        }




        //解析和查詢 類別/顏色/價格
        public async Task<ResCompleteResult> ParseAndQueryAsync(string message, CancellationToken ct = default)
        {
            try {
                //① AI 抽條件（只做一次）
                var prompt = $@"
你是一個嚴格的資料解析器，只負責從文字中抽出條件並回傳**純 JSON**。

⚠️ 請務必遵守：
- 僅輸出一個合法 JSON，**不能有任何多餘字元、註解或解釋**。
- 若無對應值請填 null，不可省略鍵。
- 僅允許以下鍵名：
  category（類別）
  color（顏色）
  price_min（價格下限）
  price_max（價格上限）

輸出格式範例：
{{""category"":""沙發"",""color"":""灰"",""price_min"":10000,""price_max"":20000}}

---

📘 規則：
1️⃣ **類別處理（允許模糊與相似詞）**
   - 主要分類：沙發, 餐桌, 餐椅, 茶几, 書桌, 書櫃, 床架, 床頭櫃, 衣櫃。
   - 若使用者輸入模糊詞或近義詞：
     - 「桌」類字 → 餐桌 或 書桌（依語意選一個最可能的）
     - 「椅」類字 → 餐椅
     - 「櫃」類字 → 書櫃 或 衣櫃（依語意選最像的）
     - 「床」類字 → 床架
     - 「沙發椅」→ 沙發
     - 其他近義詞可推論為最接近的主要分類。
   - 若實在無法推論，category = null。

2️⃣ **顏色處理（允許模糊與別名）**
   - 主要顏色：黑、白、灰、藍、綠、紅、棕、木色。
   - 若出現以下關鍵字，請對應：
     - 「深」「暗」開頭 → 對應主色（例如 深藍 → 藍）
     - 「原木」「木質」「木紋」→ 木色
     - 「咖啡」「棕色」「褐色」→ 棕
     - 「夕陽」「酒紅」「玫瑰」→ 紅
     - 「天藍」「湖水」「寶藍」→ 藍
     - 「象牙」「奶白」「米白」→ 白
   - 無法判斷則 color = null。

3️⃣ **價格處理**
   - 價格可從文字推斷單價或範圍。
   - 出現「約 / 大約 / 左右」代表模糊價格。
   - 若找不到數字，price_min 與 price_max 都回 null。

---

使用者輸入：
「{message}」

請直接回傳 JSON，不要多餘文字。
";

                var cmp = await _chat.CompleteChatAsync(prompt);
                var completion = cmp.Value; // ✅ AI 給這解法 取出真正的 ChatCompletion,需要多一層 .Value 才能取出實際內容。
                var json = completion.Content[0].Text?.Trim();
                if (string.IsNullOrWhiteSpace(json)) json = "{}";

                var parsed = JsonSerializer.Deserialize<QueryDto>(json!,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new QueryDto();

                // ② 價格範圍處理（±20% 展開）
                var (min, max) = ExpandApproxRangeIfNeeded(message, parsed.PriceMin, parsed.PriceMax);

                //③ EF：依條件查 DB（一次串到底，避免 IQueryable 跨 await）
                //decimal? min = 10000;
                //decimal? max = 12000;
                //string cat = "桌子";
                //string clr = "藍";
                var baseQuery =
                    from v in _db.TProductVariants.AsNoTracking()
                    join p in _db.TProducts.AsNoTracking() on v.FProductId equals p.FProductId
                    join c in _db.TCategories.AsNoTracking() on p.FCategoryId equals c.FCategoryId
                    join col in _db.TColors.AsNoTracking() on v.FColorId equals col.FColorId
                    select new
                    {
                        ProductId = v.FProductId,
                        Name = p.FName,
                        Category = c.FName,
                        Color = col.FColorName,
                        Price = v.FPrice
                    };

                //類別、顏色啟用過濾（用 Contains 提升容錯；若要更準可改等號）
                if (!string.IsNullOrWhiteSpace(parsed.Category))
                    //EF.Fuctions 下面執行 Like方法 此為EF Core 提供的靜態方法 
                    baseQuery = baseQuery.Where(x => EF.Functions.Like(x.Category, $"%{parsed.Category}%"));

                if (!string.IsNullOrWhiteSpace(parsed.Color))
                    baseQuery = baseQuery.Where(x => EF.Functions.Like(x.Color, $"%{parsed.Color}%"));

                if (min.HasValue) baseQuery = baseQuery.Where(x => x.Price >= min.Value);
                if (max.HasValue) baseQuery = baseQuery.Where(x => x.Price <= max.Value);

                var items = await baseQuery
                    .OrderBy(x => x.Price)
                    .Take(5)
                    .Select(x => new ProductPickDto
                    {
                        ProductId = x.ProductId,
                        Name = x.Name,
                        Category = x.Category,
                        Color = x.Color,
                        Price = x.Price,
                        //ImageUrl = x.FImage,          // 從你的 tProduct
                        //Description = x.FDescription  // 同上
                    })
                    .ToListAsync();  //(ct)

                var botText = items.Any()
                    ? "找到幾個符合條件的商品，給您參考～"
                    : "目前沒有剛好符合的商品，我可以幫您換個條件再找找看喔～";

                return new ResCompleteResult
                {
                    Parsed = new ParsedDto
                    {
                        Category = parsed.Category,
                        Color = parsed.Color,
                        PriceMin = min,
                        PriceMax = max
                    },
                    Items = items,
                    BotText = botText
                };
            }
            catch (Exception ex) {
                Console.Write(ex);
                return null;
              };
                 
             

            
        }

        // AI 解析用（Service 內部 DTO）
        private sealed class QueryDto
        {
            public string? Category { get; set; }
            public string? Color { get; set; }
            [JsonPropertyName("price_min")] public decimal? PriceMin { get; set; }
            [JsonPropertyName("price_max")] public decimal? PriceMax { get; set; }
        }

        private static (decimal? min, decimal? max) ExpandApproxRangeIfNeeded(string text, decimal? min, decimal? max)
        {
            bool approx = text.Contains("左右") || text.Contains("大約") || text.Contains("約");

            if (min.HasValue && max.HasValue)
            {
                if (approx && min.Value == max.Value)
                {
                    var v = min.Value; return (Math.Round(v * 0.8m), Math.Round(v * 1.2m));
                }
                return (min, max);
            }

            if (min.HasValue && !max.HasValue)
                return approx ? (min, Math.Round(min.Value * 1.2m)) : (min, min);

            if (!min.HasValue && max.HasValue)
                return approx ? (Math.Round(max.Value * 0.8m), max) : (max, max);

            var m = System.Text.RegularExpressions.Regex.Match(text, @"(\d{4,7})");
            if (m.Success && decimal.TryParse(m.Groups[1].Value, out var v2))
                return approx ? (Math.Round(v2 * 0.8m), Math.Round(v2 * 1.2m)) : (v2, v2);

            return (min, max);
        }
    }
}
