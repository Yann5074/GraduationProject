using GraduationProject.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GraduationProject.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<CEmployeeListItemDTO>> GetEmployeeListAsync(
        string? keyword, CancellationToken ct = default);
        Task<int> CreateEmployeeAsync(CEmployeeCreateDTO dto, CancellationToken ct = default);
    }
}
