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

        public AiProductService(dbFurniMartContext db, ChatClient chat)
        {
            _db = db;
            _chat = chat;
        }

        // 解析和查詢 類別/顏色/價格
        public async Task<ResCompleteResult> ParseAndQueryAsync(string message, CancellationToken ct )
        {
            try
            {
                // =========================
                // ① 先把「權威對照表」從 DB 撈出來（讓 AI 對齊到正確 ID）
                // =========================
                // ⭐ NEW: 從資料庫帶出「可用類別/顏色」清單
                var catList = await _db.TCategories
                    .AsNoTracking()
                    .Select(c => new { id = c.FCategoryId, name = c.FName })
                    .ToListAsync(ct);

                var colorList = await _db.TColors
                    .AsNoTracking()
                    .Select(c => new { id = c.FColorId, name = c.FColorName })
                    .ToListAsync(ct);

                // ⭐ NEW: 序列化為 JSON，嵌入提示詞
                var catalogJson = JsonSerializer.Serialize(new
                {
                    categories = catList,
                    colors = colorList
                });

                // =========================
                // ② 強化版 Prompt（要求 AI 對齊到「名稱 + ID」）
                // =========================
                var prompt = $@"
                    你是一個嚴格的『資料解析器』。你的唯一任務：從使用者中文需求中抽取條件並回傳『唯一的一個合法 JSON』。
                    不可以輸出任何多餘文字、註解、Markdown、程式碼框或尾綴。

                    ### 權威對照表（請只從以下集合挑選）：
                    {catalogJson}

                    ### 鍵名與規則（務必遵守）
                    - 僅允許這些鍵：category, category_id, color, color_id, price_min, price_max
                    - 若抽不到值，請填 null（但鍵仍要存在）
                    - category 與 color 必須是上方對照表中的 name；category_id 與 color_id 要對應該 name 的 id
                    - 一律使用繁體中文，價格單位為台幣元
                    - 不要加上任何多餘說明

                    ### 正規化規則
                    1) 類別（僅能從對照表挑選最接近的類別）
                       - 模糊詞需判斷最接近：『桌』→ 餐桌或書桌（依語意），『椅』→ 餐椅，
                         『櫃』→ 書櫃或衣櫃（依語意），『沙發椅』→ 沙發；若無法判斷 → category = 空值, category_id = 空值

                    2) 顏色（僅能從對照表挑選最接近的顏色）
                       - 深/暗開頭 → 主色（深藍→藍）
                       - 原木/木質/木紋 → 木色
                       - 咖啡/褐色 → 棕；酒紅/玫瑰 → 紅；天藍/湖水/寶藍 → 藍；象牙/奶白/米白 → 白
                       - 若無法判斷 → color = 空值, color_id = 空值

                    3) 價格（台幣元）
                       - 『萬元』= 10000、『千』= 1000
                       - A–B → price_min=A, price_max=B
                       - 『以下/以內/不超過/頂多』→ price_max；『以上/起/至少』→ price_min
                       - 『約/大約/左右』→ 先取中心值 V，再展開成 [0.8V, 1.2V] 區間
                       - 若無數字 → 兩者都為 空值

                    ### 唯一輸出格式（JSON）：
                    {{""category"":""..."",""category_id"":數字或空值,""color"":""..."",""color_id"":數字或空值,""price_min"":數字或空值,""price_max"":數字或空值}}

                    使用者輸入：
                    「{message}」
                    ";

                // 呼叫 OpenAI
                var cmp = await _chat.CompleteChatAsync(prompt);
                var completion = cmp.Value;
                var json = completion.Content[0].Text?.Trim();
                if (string.IsNullOrWhiteSpace(json)) json = "{}";

                // ⭐ NEW: 加入 category_id / color_id 反序列化
                var parsed = JsonSerializer.Deserialize<QueryDto>(
                    json!,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new QueryDto();

                // ③ 價格範圍處理（±20% 展開）
                var (min, max) = ExpandApproxRangeIfNeeded(message, parsed.PriceMin, parsed.PriceMax);

                // =========================
                // ④ EF：依條件查 DB（保守寫法，100% 可翻 SQL）
                // =========================

                // 從 Product × Variant 做單純 Join，避免難翻譯的巢狀投影
                var baseQuery =
                    from p in _db.TProducts.AsNoTracking()
                    join v in _db.TProductVariants.AsNoTracking()
                         on p.FProductId equals v.FProductId
                    where v.FPstatus == 1                    // 只取上架變體
                    select new { p, v };

                // 類別條件：先用 ID，沒有 id 才退回名稱 Like（名稱欄位請按你的實際欄位調整）
                if (parsed.CategoryId.HasValue)
                    baseQuery = baseQuery.Where(x => x.p.FCategoryId == parsed.CategoryId.Value);
                else if (!string.IsNullOrWhiteSpace(parsed.Category))
                    baseQuery = baseQuery.Where(x => EF.Functions.Like(x.p.FName ?? string.Empty, $"%{parsed.Category}%"));
                // ↑ 如果你要比對「分類名稱」，請把上面改成 x.p.Category.FName（前提：你有 Category 導覽或另外 join 了 Category）

                // 顏色條件：先用 ID，沒有 id 再退回名稱 Like（若你有 Color 名稱欄位或導覽，換成對名稱 Like）
                if (parsed.ColorId.HasValue)
                    baseQuery = baseQuery.Where(x => x.v.FColorId == parsed.ColorId.Value);
                // else if (!string.IsNullOrWhiteSpace(parsed.Color))
                //     baseQuery = baseQuery.Where(x => EF.Functions.Like(x.v.Color.ColorName, $"%{parsed.Color}%"));

                if (min.HasValue) baseQuery = baseQuery.Where(x => x.v.FPrice >= min.Value);
                if (max.HasValue) baseQuery = baseQuery.Where(x => x.v.FPrice <= max.Value);

                // 先從資料庫撈「價格最低的前 40 筆候選」——這段一定可翻 SQL
                var rows = await baseQuery
                    .OrderBy(x => x.v.FPrice)
                    .Take(40)
                    .Select(x => new
                    {
                        ProductId = x.p.FProductId,
                        CategoryId = x.p.FCategoryId,
                        ColorId = x.v.FColorId,
                        Name = x.p.FName,
                        Description = x.p.FDescription,
                        Price = x.v.FPrice,

                        // 圖片：相關子查詢取第一張圖（有排序欄位就用，沒有就直接 FirstOrDefault）
                        ImageUrl = _db.TProductAssets
                            .Where(a => a.FProductId == x.p.FProductId)
                            .OrderBy(a => a.FSortOrder ?? int.MaxValue)     // 若沒有 FSortOrder，可移除這行
                            .Select(a => a.FUrl)
                            .FirstOrDefault()

                    })

                    .ToListAsync(ct);

                // 在記憶體去重：同一個 Product 只留第一筆（因為 rows 已經按價格排序）
                var top3 = rows
                    .GroupBy(r => r.ProductId)
                    .Select(g => g.First())
                    .Take(3)
                    .Select(r => new ProductPickDto
                    {
                        ProductId = r.ProductId,
                        Name = r.Name,
                        Category = null,           // 如果需要顯示分類名稱：在上面的 baseQuery 另外 join Category，再帶進來
                        Color = null,           // 如果需要顯示顏色名稱：同理
                        Price = r.Price,
                        ImageUrl = r.ImageUrl,
                        Description = r.Description
                    })
                    .ToList();

                // 回傳文案
                var botText = top3.Any()
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
                    Items = top3,
                    BotText = botText
                };


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        // Service 內部 DTO（AI 解析用）
        private sealed class QueryDto
        {
            public string? Category { get; set; }
            [JsonPropertyName("category_id")] public int? CategoryId { get; set; }   // ⭐ NEW
            public string? Color { get; set; }
            [JsonPropertyName("color_id")] public int? ColorId { get; set; }         // ⭐ NEW
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
        //    public sealed class AiProductService : IAiProductService
        //    {
        //        private readonly dbFurniMartContext _db;
        //        private readonly ChatClient _chat;
        //        //chat client 來自 OpenAI SDK
        //        public AiProductService(dbFurniMartContext db, ChatClient chat)
        //        {
        //            _db = db;
        //            _chat = chat;
        //        }




        //        //解析和查詢 類別/顏色/價格
        //        public async Task<ResCompleteResult> ParseAndQueryAsync(string message, CancellationToken ct = default)
        //        {
        //            try {
        //                //① AI 抽條件（只做一次）
        //                var prompt = $@"
        //你是一個嚴格的資料解析器，只負責從文字中抽出條件並回傳**純 JSON**。

        //⚠️ 請務必遵守：
        //- 僅輸出一個合法 JSON，**不能有任何多餘字元、註解或解釋**。
        //- 若無對應值請填 null，不可省略鍵。
        //- 僅允許以下鍵名：
        //  category（類別）
        //  color（顏色）
        //  price_min（價格下限）
        //  price_max（價格上限）

        //輸出格式範例：
        //{{""category"":""沙發"",""color"":""灰"",""price_min"":10000,""price_max"":20000}}

        //---

        //📘 規則：
        //1️⃣ **類別處理（允許模糊與相似詞）**
        //   - 主要分類：沙發, 餐桌, 餐椅, 茶几, 書桌, 書櫃, 床架, 床頭櫃, 衣櫃。
        //   - 若使用者輸入模糊詞或近義詞：
        //     - 「桌」類字 → 餐桌 或 書桌（依語意選一個最可能的）
        //     - 「椅」類字 → 餐椅
        //     - 「櫃」類字 → 書櫃 或 衣櫃（依語意選最像的）
        //     - 「床」類字 → 床架
        //     - 「沙發椅」→ 沙發
        //     - 其他近義詞可推論為最接近的主要分類。
        //   - 若實在無法推論，category = null。

        //2️⃣ **顏色處理（允許模糊與別名）**
        //   - 主要顏色：黑、白、灰、藍、綠、紅、棕、木色。
        //   - 若出現以下關鍵字，請對應：
        //     - 「深」「暗」開頭 → 對應主色（例如 深藍 → 藍）
        //     - 「原木」「木質」「木紋」→ 木色
        //     - 「咖啡」「棕色」「褐色」→ 棕
        //     - 「夕陽」「酒紅」「玫瑰」→ 紅
        //     - 「天藍」「湖水」「寶藍」→ 藍
        //     - 「象牙」「奶白」「米白」→ 白
        //   - 無法判斷則 color = null。

        //3️⃣ **價格處理**
        //   - 價格可從文字推斷單價或範圍。
        //   - 出現「約 / 大約 / 左右」代表模糊價格。
        //   - 若找不到數字，price_min 與 price_max 都回 null。

        //---

        //使用者輸入：
        //「{message}」

        //請直接回傳 JSON，不要多餘文字。
        //";

        //                var cmp = await _chat.CompleteChatAsync(prompt);
        //                var completion = cmp.Value; // ✅ AI 給這解法 取出真正的 ChatCompletion,需要多一層 .Value 才能取出實際內容。
        //                var json = completion.Content[0].Text?.Trim();
        //                if (string.IsNullOrWhiteSpace(json)) json = "{}";

        //                var parsed = JsonSerializer.Deserialize<QueryDto>(json!,
        //                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new QueryDto();

        //                // ② 價格範圍處理（±20% 展開）
        //                var (min, max) = ExpandApproxRangeIfNeeded(message, parsed.PriceMin, parsed.PriceMax);

        //                //③ EF：依條件查 DB（一次串到底，避免 IQueryable 跨 await）
        //                //decimal? min = 10000;
        //                //decimal? max = 12000;
        //                //string cat = "桌子";
        //                //string clr = "藍";
        //                var baseQuery =
        //                    from v in _db.TProductVariants.AsNoTracking()
        //                    join p in _db.TProducts.AsNoTracking() on v.FProductId equals p.FProductId
        //                    join c in _db.TCategories.AsNoTracking() on p.FCategoryId equals c.FCategoryId
        //                    join col in _db.TColors.AsNoTracking() on v.FColorId equals col.FColorId
        //                    // LEFT JOIN 圖片表（以 VariantId 關聯）
        //                    join pa0 in _db.TProductAssets.AsNoTracking()
        //                        on v.FProductVariantId equals pa0.FProductVariantId into paJoin
        //                    from pa in paJoin.DefaultIfEmpty()
        //                    select new
        //                    {
        //                        ProductId = v.FProductId,
        //                        Name = p.FName,
        //                        Category = c.FName,
        //                        Color = col.FColorName,
        //                        Price = v.FPrice,
        //                        // 新增的兩個欄位
        //                        ImageUrl = pa != null ? pa.FUrl : null,   // 圖片 URL
        //                        Description = p.FDescription                  // 產品敘述（tProduct）
        //                    };

        //                //類別、顏色啟用過濾（用 Contains 提升容錯；若要更準可改等號）
        //                if (!string.IsNullOrWhiteSpace(parsed.Category))
        //                    //EF.Fuctions 下面執行 Like方法 此為EF Core 提供的靜態方法 
        //                    baseQuery = baseQuery.Where(x => EF.Functions.Like(x.Category, $"%{parsed.Category}%"));

        //                if (!string.IsNullOrWhiteSpace(parsed.Color))
        //                    baseQuery = baseQuery.Where(x => EF.Functions.Like(x.Color, $"%{parsed.Color}%"));

        //                if (min.HasValue) baseQuery = baseQuery.Where(x => x.Price >= min.Value);
        //                if (max.HasValue) baseQuery = baseQuery.Where(x => x.Price <= max.Value);

        //                var items = await baseQuery
        //                    .OrderBy(x => x.Price)
        //                    .Take(3)
        //                    .Select(x => new ProductPickDto
        //                    {
        //                        ProductId = x.ProductId,
        //                        Name = x.Name,
        //                        Category = x.Category,
        //                        Color = x.Color,
        //                        Price = x.Price,
        //                        ImageUrl = x.ImageUrl,          // 從你的 tProduct
        //                        Description = x.Description  // 同上
        //                    })
        //                    .ToListAsync(ct);  //(ct)

        //                var botText = items.Any()
        //                    ? "找到幾個符合條件的商品，給您參考～"
        //                    : "目前沒有剛好符合的商品，我可以幫您換個條件再找找看喔～";

        //                return new ResCompleteResult
        //                {
        //                    Parsed = new ParsedDto
        //                    {
        //                        Category = parsed.Category,
        //                        Color = parsed.Color,
        //                        PriceMin = min,
        //                        PriceMax = max
        //                    },
        //                    Items = items,
        //                    BotText = botText
        //                };
        //            }
        //            catch (Exception ex) {
        //                Console.Write(ex);
        //                return null;
        //              };




        //        }

        //        // AI 解析用（Service 內部 DTO）
        //        private sealed class QueryDto
        //        {
        //            public string? Category { get; set; }
        //            public string? Color { get; set; }
        //            [JsonPropertyName("price_min")] public decimal? PriceMin { get; set; }
        //            [JsonPropertyName("price_max")] public decimal? PriceMax { get; set; }
        //        }

        //        private static (decimal? min, decimal? max) ExpandApproxRangeIfNeeded(string text, decimal? min, decimal? max)
        //        {
        //            bool approx = text.Contains("左右") || text.Contains("大約") || text.Contains("約");

        //            if (min.HasValue && max.HasValue)
        //            {
        //                if (approx && min.Value == max.Value)
        //                {
        //                    var v = min.Value; return (Math.Round(v * 0.8m), Math.Round(v * 1.2m));
        //                }
        //                return (min, max);
        //            }

        //            if (min.HasValue && !max.HasValue)
        //                return approx ? (min, Math.Round(min.Value * 1.2m)) : (min, min);

        //            if (!min.HasValue && max.HasValue)
        //                return approx ? (Math.Round(max.Value * 0.8m), max) : (max, max);

        //            var m = System.Text.RegularExpressions.Regex.Match(text, @"(\d{4,7})");
        //            if (m.Success && decimal.TryParse(m.Groups[1].Value, out var v2))
        //                return approx ? (Math.Round(v2 * 0.8m), Math.Round(v2 * 1.2m)) : (v2, v2);

        //            return (min, max);
        //        }
        //    }


    }
