using GraduationProject.DTOs;

namespace GraduationProject.Interfaces
{
    public interface IAnalyticsService
    {
        //商品銷售排行榜
        Task<List<CBestSellerItemDTO>> GetBestSellingVariantsAsync(int top = 10, DateTime? start = null, DateTime? end = null, bool onlyCompletedOrders = true, CancellationToken ct = default);
        //訂單圖表
        Task<COrderDashboardDTO> GetDashboardAsync(CancellationToken ct = default);
        //會員圖表
        Task<CMemberGrowthDTO> GetMemberDashboardAsync(DateTime? start = null, DateTime? end = null, CancellationToken ct = default);
    }
}
