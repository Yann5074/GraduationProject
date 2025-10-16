using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IProductService
    {


        Task<List<ResProductDTO>> GetProductByProdNameAsync(string? keyword, CancellationToken ct = default);

        Task<ResultPagedDTO<ResProductListDTO>> GetAllProductsAsync(ReqProductFilterDTO filter);
        Task<ResFilterOptionsDTO> GetFilterOptionsAsync();
        Task<ResProductDetailDTO> GetProductByIdAsync(int id);
        //Task<bool> UpdateStockAsync(int variantId, int quantity);

    }

}
