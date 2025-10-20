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

            // 1) 月營收
            var monthly = await GetMonthlyRevenueAsync(ct);
            dto.MonthlyLabels = monthly.Select(x => $"{x.Year}-{x.Month:00}").ToList();
            dto.MonthlySales = monthly.Select(x => x.Total).ToList();

            //2. 訂單狀態分佈表
            var statusGroups = await _db.TOrders.AsNoTracking()
                .GroupBy(o => o.FOrderStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderBy(x => x.Status).ToListAsync(ct);

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

            dto.StatusLabels = statusGroups
                .Select(x => statusMap.TryGetValue(x.Status, out string n) ? n : $"狀態{x.Status}").ToList();
            dto.StatusCounts = statusGroups
                .Select(x => x.Count).ToList();

            //3. 付款方式分佈
            var payGroups = await _db.TOrders.AsNoTracking()
                .GroupBy(o => o.FPaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count() })
                .OrderBy(x => x.Method).ToListAsync(ct);

            Dictionary<int, string> payMap = new()
            {
                [1] = "現金支付",
                [2] = "信用卡支付",
                [3] = "行動支付"
            };
            
            dto.PaymentLabels = payGroups.Select(x => payMap.TryGetValue(x.Method, out string n) ? n : $"方式{x.Method}").ToList();
            dto.PaymentCounts = payGroups.Select(x => x.Count).ToList();

            return dto;
        }
        // ------- 圖表方法 -------

        /// <summary>月營收（合計金額）</summary>
        private async Task<List<MonthTotal>> GetMonthlyRevenueAsync(CancellationToken ct)
        {
            return await _db.TOrders.AsNoTracking()
                .GroupBy(o => new { o.FOrderTime.Year, o.FOrderTime.Month})
                .Select(g => new MonthTotal(
                    g.Key.Year,
                    g.Key.Month,
                    g.Sum(x => x.FTotalPrice)
                 ))
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync(ct);
        }
    }
}
