using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Services
{
    public class CAnalyticsService : IAnalyticsService
    {
        private readonly dbFurniMartContext _context;
        public CAnalyticsService(dbFurniMartContext context)
        {
            _context = context;
        }

        private sealed class BestSellerRow
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = default!;
            public int TotalQuantity { get; set; }
            public decimal TotalSalesAmount { get; set; }
            public int VariantCount { get; set; }
            public int? TopVariantId { get; set; }
            public string? TopVariantSKU { get; set; }
        }

        public async Task<IReadOnlyList<BestSellerDto>> GetBestSellersAsync(CBestSellerQueryDTO query, CancellationToken ct = default)
        {
            var wantTop = (query.Top <= 0 ? 5 : query.Top);
            var fetchN = Math.Max(10, wantTop * 5); // 避免後續過濾掉非上架商品後不足

            // ⚠️ 不改動原本 SQL，只在外層做程式碼過濾（商品需上架）
            var sql = @"
WITH F AS (
    SELECT 
        o.fOrderId,
        o.fOrderTime,
        o.fOrderStatus,
        od.fProductVariantId,
        od.fQuantity,
        od.fUnitPrice
    FROM dbo.tOrderDetail od
    INNER JOIN dbo.tOrder o ON o.fOrderId = od.fOrderId
    WHERE 
        od.fIsDeleted = 0
        AND o.fIsDeleted = 0
        AND (@FromUtc IS NULL OR o.fOrderTime >= @FromUtc)
        AND (@ToUtc   IS NULL OR o.fOrderTime <  @ToUtc)
        AND EXISTS (
            SELECT 1
            FROM string_split(@IncludeStatuses, ',') ss
            WHERE TRY_CAST(ss.value AS int) = o.fOrderStatus
        )
)
SELECT TOP (@TopN)
    p.fProductId               AS ProductId,
    p.fName                    AS ProductName,
    SUM(F.fQuantity)           AS TotalQuantity,
    SUM(F.fQuantity * F.fUnitPrice) AS TotalSalesAmount,
    COUNT(DISTINCT pv.fProductVariantId) AS VariantCount,
    MAX(CASE WHEN rv.rn = 1 THEN pv.fProductVariantId END) AS TopVariantId,
    MAX(CASE WHEN rv.rn = 1 THEN pv.fSKU END)              AS TopVariantSKU
FROM F
INNER JOIN dbo.tProductVariant pv ON pv.fProductVariantId = F.fProductVariantId
INNER JOIN dbo.tProduct p ON p.fProductId = pv.fProductId
OUTER APPLY (
    SELECT 
        ROW_NUMBER() OVER (ORDER BY SUM(F2.fQuantity) DESC) AS rn,
        pv2.fProductVariantId,
        pv2.fSKU
    FROM F F2
    INNER JOIN dbo.tProductVariant pv2 ON pv2.fProductVariantId = F2.fProductVariantId
    WHERE pv2.fProductId = p.fProductId
    GROUP BY pv2.fProductVariantId, pv2.fSKU
) rv
GROUP BY p.fProductId, p.fName
ORDER BY 
    TotalQuantity DESC,
    TotalSalesAmount DESC;";

            var pFrom = new SqlParameter("@FromUtc", (object?)query.From ?? DBNull.Value);
            var pTo = new SqlParameter("@ToUtc", (object?)query.To ?? DBNull.Value);
            var pTop = new SqlParameter("@TopN", fetchN);
            var pSta = new SqlParameter("@IncludeStatuses", string.IsNullOrWhiteSpace(query.Statuses) ? "2,3,4,5" : query.Statuses);

            var raw = await _context.Database
                .SqlQueryRaw<BestSellerRow>(sql, pFrom, pTo, pTop, pSta)
                .ToListAsync(ct);

            if (raw.Count == 0) return Array.Empty<BestSellerDto>();

            // 僅保留「上架中」商品（tProduct.fPStatus = 1）
            var productIds = raw.Select(r => r.ProductId).Distinct().ToArray();

            var onlineIds = await _context.Set<TProduct>()
                .Where(p => productIds.Contains(p.FProductId) && p.FPstatus == 1)
                .Select(p => p.FProductId)
                .ToListAsync(ct);

            var online = onlineIds.ToHashSet();

            var result = raw
                .Where(r => online.Contains(r.ProductId))
                .Take(wantTop)
                .Select(r => new BestSellerDto(
                    r.ProductId,
                    r.ProductName,
                    r.TotalQuantity,
                    r.TotalSalesAmount,
                    r.VariantCount,
                    r.TopVariantId,
                    r.TopVariantSKU
                ))
                .ToList();

            return result;
        }
    }
}
