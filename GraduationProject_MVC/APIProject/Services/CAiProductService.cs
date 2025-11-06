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

        public async Task<ResCompleteResult> ParseAndQueryAsync(string message, CancellationToken ct = default)
        {
            // ① AI 抽條件（只做一次）
            var prompt = $@"
只回 JSON（不要多餘文字），格式：
{{""category"":""類別"",""color"":""顏色"",""price_min"":最小或null,""price_max"":最大或null}}
使用者說：「{message}」";

            var cmp = await _chat.CompleteChatAsync(prompt);
            var completion = cmp.Value; // ✅ AI 給這解法 取出真正的 ChatCompletion,需要多一層 .Value 才能取出實際內容。
            var json = completion.Content[0].Text?.Trim();
            if (string.IsNullOrWhiteSpace(json)) json = "{}";

            var parsed = JsonSerializer.Deserialize<QueryDto>(json!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new QueryDto();

            // ② 價格範圍處理（±20% 展開）
            var (min, max) = ExpandApproxRangeIfNeeded(message, parsed.PriceMin, parsed.PriceMax);

            // ③ EF：依條件查 DB（一次串到底，避免 IQueryable 跨 await）
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

            if (!string.IsNullOrWhiteSpace(parsed.Category))
                baseQuery = baseQuery.Where(x => x.Category.Contains(parsed.Category!));

            if (!string.IsNullOrWhiteSpace(parsed.Color))
                baseQuery = baseQuery.Where(x => x.Color.Contains(parsed.Color!));

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
                    Price = x.Price
                })
                .ToListAsync(ct);

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
