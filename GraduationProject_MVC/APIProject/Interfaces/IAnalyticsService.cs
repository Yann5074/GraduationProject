using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IAnalyticsService
    {
        Task<IReadOnlyList<CBestSellerDTO>> GetBestSellersAsync(CBestSellerQueryDTO query, CancellationToken ct = default);
    }
}
