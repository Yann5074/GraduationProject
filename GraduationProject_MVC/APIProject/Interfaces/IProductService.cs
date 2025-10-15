using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IProductService
    {
        Task<List<ResProductDTO>> AllProductAsync(CancellationToken ct = default);
    }
}
