using GraduationProject.DTOs;

namespace GraduationProject.Interfaces
{
    public interface IProductService
    {
        Task<CProductDTO?> GetAsync(int productId, CancellationToken ct = default);
        Task<IReadOnlyList<CProductDTO>> ListAsync(CancellationToken ct = default);
        Task<CProductDTO> CreateAsync(CProductCreateDTO input, CancellationToken ct = default);
        Task<CProductDTO> UpdateAsync(CProductUpdateDTO input, CancellationToken ct = default);
        Task<bool> DeleteAsync(int productId, CancellationToken ct = default);
    }
}
