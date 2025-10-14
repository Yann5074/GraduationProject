using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IProductService
    {
        Task<ResultPagedDTO<ResProductDTO>> GetProductsAsync(ReqProductQueryDTO query, CancellationToken ct = default);
    }
}
