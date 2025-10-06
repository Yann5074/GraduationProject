using GraduationProject.DTOs;
using GraduationProject.Queries;

namespace GraduationProject.Interfaces
{
    public interface IProductService
    {
        Task<CProductDTO?> GetAsync(int productId, CancellationToken ct = default);
        Task<IReadOnlyList<CProductDTO>> ListAsync(CancellationToken ct = default);
        
        Task<IReadOnlyList<CProductDTO>> SearchAsync(string? keyword, CancellationToken ct = default);
        Task<ProductDataTableResult> DataTableAsync(ProductDataTableQuery q, CancellationToken ct = default);
    }

}
