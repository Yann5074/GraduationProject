using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IProductService
    {


        Task<List<ResProductDTO>> GetProductByProdNameAsync(string? keyword, CancellationToken ct = default);
        Task<ResultPagedDTO<ResProductListDTO>> GetAllProductsAsync(ReqProductFilterDTO filter);
        Task<ResProductDetailDTO> GetProductByIdAsync(int id, bool includeCustomization = false);
        Task<ResFilterOptionsDTO> GetFilterOptionsAsync();
        
        Task<List<ResProductListDTO>> GetSimilarProductsAsync(int productId, int count = 4);
        // 取得產品的完整 3D 自訂資訊
        //Task<ResProductCustomizationDTO> GetProductCustomizationAsync(int productId);

        //載入購物車頁面
        //批次取得購物車商品資訊
        // 取得單一產品變體資訊（給購物車用） 
        //如:加入購物車時
        Task<List<ResCartProductDTO>> GetCartProductsAsync(List<int> productVariantIds);

        //價格和庫存資訊
        Task<ResProductPriceDTO> GetPriceByCustomizationAsync(
            int productId,
            Dictionary<string, int> selectedOptions);

        //庫存檢查結果
        Task<ResStockCheckDTO> CheckStockAsync(List<ReqStockCheckItemDTO> items);
    }

}
