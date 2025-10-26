using GraduationProject.DTOs;

namespace GraduationProject.Interfaces
{
    public interface ILeaveService
    {
        // 申請
        Task<int> CreateAsync(CLeaveCreateDTO dto, CancellationToken ct = default);

        // 申請人清單
        Task<List<CLeaveItemDTO>> GetMyLeavesAsync(int employeeId, CancellationToken ct = default);

        //軟刪清單
        // 申請人清單
        Task<List<CLeaveItemDTO>> GetMyDeletedLeavesAsync(int employeeId, CancellationToken ct = default);

        // 主管審核清單
        Task<List<CLeaveItemDTO>> GetPendingAsync(CancellationToken ct = default);

        // 核准/駁回
        Task<bool> ApproveAsync(int leaveId, int approverId, CancellationToken ct = default);
        Task<bool> RejectAsync(int leaveId, int approverId, string? reason = null, CancellationToken ct = default);

        // 取得單筆
        Task<CLeaveItemDTO?> GetByIdAsync(int leaveId, CancellationToken ct = default);

        //軟刪
        Task<bool> SoftDeletedAsync(int leaveId, CancellationToken ct = default);

        //硬刪
        Task<bool> DeleteLeavesAsync(int id, CancellationToken ct = default);
    }
}
