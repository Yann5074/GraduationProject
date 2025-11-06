using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IRecommonedService
    {
        public Task<IReadOnlyList<ResRecommonedDTO>> ItemToItemRecommonedAsync(int productId, int take = 12, int variantsPerProduct = 2, int windowDay = 30, int sessionWindowMinutes = 30, CancellationToken ct = default);
       
        public Task<IReadOnlyList<ResRecommonedDTO>> TrendingAsync(int take = 12, int variantsPerProduct = 2, int windowDay = 30, CancellationToken ct = default);
        
        public Task<IReadOnlyList<ResRecommonedDTO>> ForSessionAsync(string sessionId, int take = 12, int variantPerProduct = 2, int windowDay = 30, int sessionWindowMinutes = 30, CancellationToken ct = default);
        
        public Task InvalidateAllAsync();
    }
}
