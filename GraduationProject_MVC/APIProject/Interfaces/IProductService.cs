using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IProductService
    {


        Task<ResultPagedDTO<ResProductListDTO>> GetAllProductsAsync(ReqProductFilterDTO filter);
        Task<List<ResProductDTO>> GetProductByProdNameAsync(string? keyword, CancellationToken ct = default);
        Task<ResFilterOptionsDTO> GetFilterOptionsAsync();
        Task<ResProductDetailDTO?> GetProductByIdAsync(int id, bool includeCustomization = false);
        Task<List<ResProductListDTO>> GetSimilarProductsAsync(int productId, int count = 4);
        Task<List<ResCartProductDTO>> GetCartProductsAsync(List<int> productVariantIds);
        Task<ResProductPriceDTO?> GetPriceByCustomizationAsync(int productId, Dictionary<string, int> selectedOptions);
        Task<ResStockCheckDTO> CheckStockAsync(List<ReqStockCheckItemDTO> items);
    }

}
