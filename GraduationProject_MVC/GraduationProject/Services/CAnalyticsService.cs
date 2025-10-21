using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    // 描述「年月 + 總金額」
    public record MonthTotal(int Year, int Month, decimal Total);
    // 描述「年月 + 筆數」
    public record MonthCount(int Year, int Month, int Count);
    // 描述「名稱 + 筆數」（例如 狀態、付款方式）
    public record NameCount(string Name, int Count);

    public class CAnalyticsService : IAnalyticsService
    {
        private readonly dbFurniMartContext _db;
        public CAnalyticsService(dbFurniMartContext db) => _db = db;

        public async Task<COrderDashboardDTO> GetDashboardAsync(CancellationToken ct = default)
        {
            var dto = new COrderDashboardDTO();

            // 1. 月營收
            var monthly = await GetMonthlyRevenueAsync(ct);
            dto.MonthlyLabels = monthly.Select(x => $"{x.Year}-{x.Month:00}").ToList();
            dto.MonthlySales = monthly.Select(x => x.Total).ToList();

            //2. 訂單狀態分佈表
            var statusGroups = await GetOrderStatusAsync(ct);
            dto.StatusLabels = statusGroups.Select(x => x.Name).ToList();
            dto.StatusCounts = statusGroups.Select(x => x.Count).ToList();

            //3. 付款方式分佈
            var payGroups = await GetOrderPayGroupAsync(ct);
            dto.PaymentLabels = payGroups.Select(x => x.Name).ToList();
            dto.PaymentCounts = payGroups.Select(x => x.Count).ToList();

            // 4. 成交量（完成）最高/最低月份
            var (maxCompleted, minCompleted) = await GetCompletedMaxMinMonthsAsync(ct);
            dto.MaxCompletedMonths = maxCompleted.Select(x =>
                new MonthCountItemDTO { Label = $"{x.Year}-{x.Month:00}", Count = x.Count }).ToList();
            dto.MinCompletedMonths = minCompleted.Select(x =>
                new MonthCountItemDTO { Label = $"{x.Year}-{x.Month:00}", Count = x.Count }).ToList();

            // 5. 棄單（取消/刪除）最高/最低月份
            var (maxCanceled, minCanceled) = await GetCanceledMaxMinMonthsAsync(ct);
                dto.MaxCanceledMonths = maxCanceled.Select(x => new MonthCountItemDTO {Label = $"{x.Year}-{x.Month:00}", Count = x.Count}).ToList();
                dto.MinCanceledMonths = minCanceled.Select(x => new MonthCountItemDTO { Label = $"{x.Year}-{x.Month:00}", Count = x.Count }).ToList();

            return dto;
        }

        // ------- 圖表方法 -------

        /// <summary>月營收（合計金額）</summary>
        private async Task<List<MonthTotal>> GetMonthlyRevenueAsync(CancellationToken ct)
        {
            // 1) 全部在資料庫做 GroupBy/合計（只用匿名型別）
            var rows = await _db.TOrders
                .AsNoTracking()
                .GroupBy(o => new { Year = o.FOrderTime.Year, Month = o.FOrderTime.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    // 若非 nullable，改成 g.Sum(x => x.FTotalPrice)
                    Total = g.Sum(x => (decimal?)x.FTotalPrice) ?? 0m
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync(ct);

            // 2) 回到記憶體再 new
            return rows.Select(x => new MonthTotal(x.Year, x.Month, x.Total)).ToList();
        }

        /// <summary>訂單狀態分佈（筆數）</summary>
        private async Task<List<NameCount>> GetOrderStatusAsync(CancellationToken ct)
        {
            Dictionary<int, string> statusMap = new()
            {
                [1] = "處理中",
                [2] = "訂單成立",
                [3] = "付款資訊確認",
                [4] = "訂單出貨",
                [5] = "完成訂單",
                [6] = "訂單取消",
                [7] = "訂單刪除"
            };

            var rows = await _db.TOrders.AsNoTracking()
                .GroupBy(o => o.FOrderStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderBy(x => x.Status)
                .ToListAsync(ct);

            return rows
            .Where(x => statusMap.ContainsKey(x.Status)) // 過濾未知碼
            .Select(x => new NameCount(statusMap[x.Status], x.Count))
            .ToList();
        }

        /// <summary>付款方式分佈（筆數）</summary>
        private async Task<List<NameCount>> GetOrderPayGroupAsync(CancellationToken ct)
        {
            Dictionary<int, string> payMap = new()
            {
                [1] = "現金支付",
                [2] = "信用卡支付",
                [3] = "行動支付"
            };

            var rows = await _db.TOrders.AsNoTracking()
                .GroupBy(o => o.FPaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count() })
                .OrderBy(x => x.Method).ToListAsync(ct);

            return rows
                .Where(x => payMap.ContainsKey(x.Method))
                .Select(x => new NameCount(payMap[x.Method], x.Count))
                .ToList();
        }

        /// <summary>完成訂單（月）Max/Min（含同值 tie）</summary>
        public async Task<(List<MonthCount> Max, List<MonthCount> Min)> GetCompletedMaxMinMonthsAsync(CancellationToken ct)
        {
            var rows = await _db.TOrders.AsNoTracking()
            .Where(o => o.FOrderStatus == 5) // 完成
            .GroupBy(o => new { o.FOrderTime.Year, o.FOrderTime.Month })
            .Select(g => new MonthCount(g.Key.Year, g.Key.Month, g.Count()))
            .ToListAsync(ct);

            if (rows.Count == 0) return (new(), new());

            var max = rows.Max(x => x.Count);
            var min = rows.Min(x => x.Count);
            return (
                rows.Where(x => x.Count == max).OrderBy(x => x.Year).ThenBy(x => x.Month).ToList(),
                rows.Where(x => x.Count == min).OrderBy(x => x.Year).ThenBy(x => x.Month).ToList()
            );
        }

        /// <summary>棄單（月）Max/Min（取消/刪除，含同值 tie）</summary>
        public async Task<(List<MonthCount> Max, List<MonthCount> Min)> GetCanceledMaxMinMonthsAsync(CancellationToken ct)
        {
            int[] canceled = { 6, 7 };
            var rows = await _db.TOrders.AsNoTracking()
                .Where(o => canceled.Contains(o.FOrderStatus))
                .GroupBy(o => new { o.FOrderTime.Year, o.FOrderTime.Month })
                .Select(g => new MonthCount(g.Key.Year, g.Key.Month, g.Count()))
                .ToListAsync(ct);

            if (rows.Count == 0) return (new(), new());

            var max = rows.Max(x => x.Count);
            var min = rows.Min(x => x.Count);
            return (
                rows.Where(x => x.Count == max).OrderBy(x => x.Month).ToList(),
                rows.Where(x => x.Count == min).OrderBy(x => x.Month).ToList()
            );
        }
    }
}
