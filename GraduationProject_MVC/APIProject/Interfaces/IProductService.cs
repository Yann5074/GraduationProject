using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IProductService
    {


        Task<ResultPagedDTO<ResProductListDTO>> GetAllProductsAsync(ReqProductFilterDTO filter);
        Task<List<ResProductDTO>> GetProductByProdNameAsync(string? keyword, CancellationToken ct = default);
        Task<ResFilterOptionsDTO> GetFilterOptionsAsync();
        Task<ResProductDetailDTO?> GetProductByIdAsync(int id);
        // 回傳此產品所有可用 PBR 材質（每個顏色/變體一筆；BaseColor 隨顏色不同）
        Task<List<ResPBRMaterialDTO>> GetPBRMaterialsAsync(int productId, HttpContext http);
        // 回傳預設 PBR（沒有指定顏色時要用的那一筆）
        Task<ResPBRMaterialDTO?> GetDefaultPBRAsync(int productId, HttpContext http);
        // 回傳產品的變體列表（含 color 資訊），供前端渲染顏色選擇
        Task<List<ResVariantDTO>> GetVariantsAsync(int productId);
        Task<List<ResProductListDTO>> GetSimilarProductsAsync(int productId, int count = 4);
        Task<List<ResCartProductDTO>> GetCartProductsAsync(List<int> productVariantIds);
        Task<ResProductPriceDTO?> GetPriceByCustomizationAsync(int productId, Dictionary<string, int> selectedOptions);
        Task<ResStockCheckDTO> CheckStockAsync(List<ReqStockCheckItemDTO> items);
    }

}
