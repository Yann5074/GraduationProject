using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IProductService
    {
        Task<List<ResProductDTO>> GetAllProductAsync(CancellationToken ct = default);

        Task<List<ResProductDTO>> GetProductByProdNameAsync(string? keyword, CancellationToken ct = default);
    }

}
