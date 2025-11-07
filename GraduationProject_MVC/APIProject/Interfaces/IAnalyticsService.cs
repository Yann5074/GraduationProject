using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IAnalyticsService
    {
        Task<IReadOnlyList<BestSellerDto>> GetBestSellersAsync(CBestSellerQueryDTO query, CancellationToken ct = default);
    }
}
