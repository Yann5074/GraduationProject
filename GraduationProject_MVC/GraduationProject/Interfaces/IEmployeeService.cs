using GraduationProject.DTOs;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GraduationProject.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<CEmployeeListItemDTO>> GetEmployeeListAsync(
        string? keyword, CancellationToken ct = default);
        Task<int> CreateEmployeeAsync(CEmployeeCreateDTO dto, CancellationToken ct = default);
        Task<bool> DeleteEmployeeAsync(int? id);
        Task<bool> EditEmployeeAsync(int id, CEmployeeEditDTO dto, CancellationToken ct = default);
        // 讀取 Edit 畫面的資料（直接回 VM，找不到回 null）
        Task<CEmployeeEditViewModel?> GetEmployeeEditVmAsync(int id, CancellationToken ct = default);
    }
}
