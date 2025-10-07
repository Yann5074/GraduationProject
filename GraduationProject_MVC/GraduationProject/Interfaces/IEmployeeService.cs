using GraduationProject.DTOs;

namespace GraduationProject.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<CEmployeeListItemDTO>> GetEmployeeListAsync(
        string? keyword, CancellationToken ct = default);
    }
}
