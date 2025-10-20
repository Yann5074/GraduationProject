using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IProductService
    {


        Task<List<ResProductDTO>> GetProductByProdNameAsync(string? keyword, CancellationToken ct = default);
        Task<ResultPagedDTO<ResProductListDTO>> GetAllProductsAsync(ReqProductFilterDTO filter);
        Task<ResFilterOptionsDTO> GetFilterOptionsAsync();
        Task<ResProductDetailDTO> GetProductByIdAsync(int id);
        Task<List<ResProductListDTO>> GetSimilarProductsAsync(int productId, int count = 4);
        // 取得產品的完整 3D 自訂資訊
        Task<ResProductCustomizationDTO> GetProductCustomizationAsync(int productId);
    }

}
