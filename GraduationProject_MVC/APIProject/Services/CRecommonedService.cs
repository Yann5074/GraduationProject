using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using ApiProject.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
using System.Drawing;
using System.Reflection.Emit;

namespace ApiProject.Services
{
    // #TODO
    public class CRecommonedService : IRecommonedService
    {
        private readonly dbFurniMartContext _context;
        private readonly IMemoryCache _cache;
        // 設定TTL為10分鐘
        private readonly MemoryCacheEntryOptions _opt = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            Size = 1
        };
        public CRecommonedService(dbFurniMartContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // 共訪推薦方法
        public async Task<IReadOnlyList<ResRecommonedDTO>> ItemToItemRecommonedAsync(int productId, int take = 12, int variantsPerProduct = 2, int windowDay = 30, int sessionWindowMinutes = 30, CancellationToken ct = default)
        {
            var ver = RecommendedCacheVersion.Current;
            string key = $"it2it:v{ver}:p{productId}:n{take}:k{variantsPerProduct}:w{windowDay}:sw{sessionWindowMinutes}";
            if (_cache.TryGetValue(key, out IReadOnlyList<ResRecommonedDTO>? cached) && cached is not null)
                return cached;

            // 擷取視窗天數內事件
            var events = await LoadNormalizedEventsAsync(windowDay, ct);

            // 共同訪問計分
            var co = BuildCoVisition(events, sessionWindowMinutes);

            // 獲取與目標商品相似的產品清單 (降序排列)
            var neighborProductIds = co.TryGetValue(productId, out var map) ? map.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList() : new List<int>();

            if (neighborProductIds.Count == 0)
            {
                var hotProductIds = await _context.Set<TUserEvent>()
                    .AsNoTracking()
                    .Where(e => e.FOccurredAt >= DateTime.UtcNow.AddDays(-windowDay) && e.FProductId != null)
                    .GroupBy(e => e.FProductId!.Value)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .Take(20)
                    .ToListAsync(ct);
                var resulthot = await ExpandProductsToVariantAsync(hotProductIds, variantsPerProduct, windowDay, take, ct);
                _cache.Set(key, resulthot, _opt);
                return resulthot;
            }

            // 挑選前N個變體，插入take
            var result = await ExpandProductsToVariantAsync(neighborProductIds, variantsPerProduct, windowDay, take, ct);

            _cache.Set(key, result, _opt);
            return result;
        }

        // 手動更新方法
        public Task InvalidateAllAsync()
        {
            RecommendedCacheVersion.Bump();
            return Task.CompletedTask;
        }

        // 定義內部事件行
        private sealed class EventRow
        {
            public int? ProductId { get; set; }
            public int? ProductVariantId { get; set; }
            public string? SessionId { get; set; }
            public int? UserId { get; set; }
            public int EventType { get; set; }
            public DateTime OccuredAt { get; set; }
        }

        // 查詢資料 (內部)
        private async Task<List<EventRow>> LoadNormalizedEventsAsync(int windowDay, CancellationToken ct)
        {
            var since = DateTime.UtcNow.AddDays(-windowDay);

            //只抓用的到的欄位資訊
            var rows = await _context.Set<TUserEvent>()
                .AsNoTracking()
                .Where(e => e.FOccurredAt >= since)
                .Select(e => new
                {
                    ProductId = e.FProductId ?? (e.FProductVariantId != null ? e.Product.FProductId : (int?)null),
                    e.FProductVariantId,
                    e.FSessionId,
                    e.FUserId,
                    e.FEventType,
                    e.FOccurredAt
                })
                .Where(x => x.ProductId != null)
                .ToListAsync(ct);

            // 映射到內部輕量模型
            return rows.Select(x => new EventRow
            {
                ProductId = x.ProductId,
                ProductVariantId = x.FProductVariantId,
                SessionId = x.FSessionId,
                UserId = x.FUserId,
                EventType = x.FEventType,
                OccuredAt = x.FOccurredAt
            }).ToList();
        }

        // 把商品清單展開成變體在裝成take
        private async Task<IReadOnlyList<ResRecommonedDTO>> ExpandProductsToVariantAsync(List<int> productIds, int variantsPerProduct, int windowDay, int take, CancellationToken ct)
        {
            var result = new List<ResRecommonedDTO>();
            if (productIds.Count == 0)
                return result;

            var since = DateTime.UtcNow.AddDays(-windowDay);

            //抓產品名稱
            var nameMap = await _context.Set<TProduct>()
                .AsNoTracking()
                .Where(p => productIds.Contains(p.FProductId))
                .Select(p => new
                {
                    p.FProductId,
                    p.FName
                })
                .ToDictionaryAsync(x => x.FProductId, x => x.FName, ct);

            // 計算視窗時間內的權重分數
            var variantScores = await _context.Set<TUserEvent>()
                .AsNoTracking()
                .Where(e => e.FOccurredAt >= since && e.FProductVariantId != null)
                .GroupBy(e => e.FProductVariantId!.Value)
                .Select(g => new
                {
                    VariantId = g.Key,
                    Score = g.Sum(e => e.FEventType == 3 ? 5 : e.FEventType == 2 ? 2 : e.FEventType == 1 ? 1 : 0)
                })
                .ToListAsync(ct);

            var scoreByVariant = variantScores.ToDictionary(x => x.VariantId, x => (double)x.Score);

            //取得清單下所有變體
            // 先抓出基本變體與產品資料
            var variantBase = await (
                from v in _context.Set<TProductVariant>().AsNoTracking()
                join p in _context.Set<TProduct>().AsNoTracking()
                    on v.FProductId equals p.FProductId
                where productIds.Contains((int)v.FProductId)
                select new
                {
                    v.FProductVariantId,
                    v.FProductId,
                    p.FName,
                    v.FLength,
                    v.FWidth,
                    v.FHeight,
                    v.FWeight
                }
            ).ToListAsync(ct);

            // 取出所有變體 ID
            var variantIds = variantBase.Select(x => x.FProductVariantId).ToList();
            var productIdsForAssets = variantBase.Select(x => x.FProductId).Distinct().ToList();

            // 查變體層的圖片（優先順序：主圖 > sortOrder）
            var variantAssetMap = await _context.Set<TProductAsset>()
                .AsNoTracking()
                .Where(a => a.FProductVariantId != null && variantIds.Contains(a.FProductVariantId.Value))
                .GroupBy(a => a.FProductVariantId)
                .Select(g => new
                {
                    ProductVariantId = g.Key!.Value,
                    ImageUrl = g.OrderByDescending(x => x.FIsPrimary)
                                .ThenBy(x => x.FSortOrder)
                                .Select(x => x.FUrl ?? x.FPosterUrl)
                                .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.ProductVariantId, x => x.ImageUrl, ct);

            // 查產品層的圖片（若變體沒圖就用產品封面）
            var productAssetMap = await _context.Set<TProductAsset>()
                .AsNoTracking()
                .Where(a => productIdsForAssets.Contains(a.FProductId))
                .GroupBy(a => a.FProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ImageUrl = g.OrderByDescending(x => x.FIsPrimary)
                                .ThenBy(x => x.FSortOrder)
                                .Select(x => x.FUrl ?? x.FPosterUrl)
                                .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.ImageUrl, ct);

            // 組合最終結果
            var variants = variantBase.Select(v => new
            {
                ProductVariantId = v.FProductVariantId,
                ProductId = v.FProductId,
                Name = v.FName,
                Size = $"{v.FLength} x {v.FWidth} x {v.FHeight} cm / {v.FWeight} Kg",
                ImageUrl = variantAssetMap.GetValueOrDefault(v.FProductVariantId)
                         ?? productAssetMap.GetValueOrDefault(v.FProductId)
            }).ToList();

            foreach (var pid in productIds)
            {
                var list = variants.Where(v => v.ProductId == pid)
                    .OrderByDescending(v => scoreByVariant.GetValueOrDefault(v.ProductVariantId, 0.0))
                    .ThenBy(v => v.ProductVariantId)
                    .Take(variantsPerProduct);

                foreach(var v in list)
                {
                    result.Add(new ResRecommonedDTO
                    {
                        ProductId = (int)v.ProductId,
                        ProductVariantId = v.ProductVariantId,
                        Name = v.Name,
                        Size = v.Size,
                        ImageUrl = v.ImageUrl,
                    });
                    if (result.Count >= take)
                        return result;
                }
            }
            return result;
        }

        // 共同訪問計分
        private static Dictionary<int, Dictionary<int, double>> BuildCoVisition(List<EventRow> events, int sessionWindowMinutes)
        {
            static double W(int t) => t == 3 ? 5.0 : 5 == 2 ? 2.0 : t == 1 ? 1.0 : 0.0;

            var bySession = events
                .Where(e => !string.IsNullOrEmpty(e.SessionId))
                .GroupBy(e => e.SessionId);

            var co = new Dictionary<int, Dictionary<int, double>>();

            foreach (var sess in bySession)
            {
                var seq = sess.OrderBy(e => e.OccuredAt).ToList();
                for (int i = 0; i < seq.Count; i++)
                {
                    var a = seq[i];
                    for (int j = i+1; j < seq.Count; j++)
                    {
                        var b = seq[j];
                        if ((b.OccuredAt - a.OccuredAt).TotalMinutes > sessionWindowMinutes)
                            break;
                        if (a.ProductId is null || b.ProductId is null)
                            continue;
                        if (a.ProductId == b.ProductId)
                            continue;

                        var score = W(a.EventType) + W(b.EventType);

                        if (!co.TryGetValue(a.ProductId.Value, out var mapA))
                            co[a.ProductId.Value] = mapA = new Dictionary<int, double>();
                        if (!co.TryGetValue(b.ProductId.Value, out var mapB))
                            co[b.ProductId.Value] = mapB = new Dictionary<int, double>();

                        mapA[b.ProductId.Value] = mapA.GetValueOrDefault(b.ProductId.Value) + score;
                        mapB[a.ProductId.Value] = mapB.GetValueOrDefault(a.ProductId.Value) + score;
                    }
                }
            }
            return co;
        }

        // 熱門啟動方法
        public async Task<IReadOnlyList<ResRecommonedDTO>> TrendingAsync(int take = 12, int variantsPerProduct = 2, int windowDay = 30, CancellationToken ct = default)
        {
            var ver = RecommendedCacheVersion.Current;
            var key = $"trend:v{ver}:n{take}:k{variantsPerProduct}:w{windowDay}";
            if (_cache.TryGetValue(key, out IReadOnlyList<ResRecommonedDTO>? cached) && cached is not null)
                return cached;

            var since = DateTime.UtcNow.AddDays(-windowDay);

            //進 N 日內互動最多的產品 (view=1, add=2, buy=5)
            var hotProductIds = await _context.Set<TUserEvent>()
                .AsNoTracking()
                .Where(e => e.FOccurredAt >= since && e.FProductId != null)
                .GroupBy(e => e.FProductId!.Value)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Score = g.Sum(e => e.FEventType == 3 ? 5 : e.FEventType == 2 ? 2 : e.FEventType == 1 ? 1 : 0)
                }).OrderByDescending(x => x.Score)
                .Select(x => x.ProductId)
                .Take(50)
                .ToListAsync(ct);

            var result = await ExpandProductsToVariantAsync(hotProductIds, variantsPerProduct, windowDay, take, ct);
            _cache.Set(key, result, _opt);
            return result;
        }

        // 訪客啟動方法
        public async Task<IReadOnlyList<ResRecommonedDTO>> ForSessionAsync(string sessionId, int take = 12, int variantPerProduct = 2, int windowDay = 30, int sessionWindowMinutes = 30, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return Array.Empty<ResRecommonedDTO>();

            var ver = RecommendedCacheVersion.Current;
            var key = $"sess:v{ver}:s{sessionId}:n{take}:k{variantPerProduct}:w{windowDay}:sw{sessionWindowMinutes}";
            if (_cache.TryGetValue(key, out IReadOnlyList<ResRecommonedDTO>? cached) && cached is not null)
                return cached;

            // 取近N日事件計算共訪
            var events = await LoadNormalizedEventsAsync(windowDay, ct);
            var co = BuildCoVisition(events, sessionWindowMinutes);

            //獲取該session最近互動的 product 列表
            var since = DateTime.UtcNow.AddDays(-windowDay);
            var recentProducts = await _context.Set<TUserEvent>()
                .AsNoTracking()
                .Where(e => e.FSessionId == sessionId && e.FOccurredAt >= since)
                .OrderByDescending(e => e.FOccurredAt)
                .Select(e => e.FProductId ?? e.ProductVariant.FProductId)
                .Where(pid => pid != null)
                .Select(pid => pid!.Value)
                .Distinct()
                .Take(10)
                .ToListAsync(ct);

            //透過 co-visitation 展開鄰居
            var neighbors = new List<int>();
            foreach(var pid in recentProducts)
            {
                if (!co.TryGetValue(pid, out var map))
                    continue;
                neighbors.AddRange(map
                    .OrderByDescending(kv => kv.Value)
                    .Select(kv => kv.Key)
                    );
            }

            var neighborProductIds = neighbors
                .Where(id => id != 0)
                .GroupBy(id => id)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Distinct()
                .Take(50)
                .ToList();

            if (neighborProductIds.Count == 0)
                return await TrendingAsync(take, variantPerProduct, windowDay, ct);

            var result = await ExpandProductsToVariantAsync(neighborProductIds, variantPerProduct, windowDay, take, ct);
            _cache.Set(key, result, _opt);
            return result;
        }

    }
}
