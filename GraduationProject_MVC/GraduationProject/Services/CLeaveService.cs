using GraduationProject.DTOs;
using GraduationProject.Enum;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    public class CLeaveService : ILeaveService
    {
        private readonly dbFurniMartContext _db;
        public CLeaveService(dbFurniMartContext db) => _db = db;

        //List
        public async Task<List<CLeaveItemDTO>> GetMyLeavesAsync(int employeeId, CancellationToken ct = default)
        {
            return await _db.TLeaves.AsNoTracking()
                .Where(x => x.FEmployeeId == employeeId && x.FStatusId != (int)LeaveRequestStatusEnum.Deleted)
                .OrderByDescending(x => x.FCreatetime)
                .Select(x => new CLeaveItemDTO
                {
                    LeaveId = x.FLeaveId,
                    EmployeeId = (int)x.FEmployeeId,
                    EmployeeName = _db.TEmployees
                                      .Where(e => e.FEmployeeId == x.FEmployeeId)
                                      .Select(e => e.FName).FirstOrDefault() ?? "",
                    LeaveType = x.FLeaveType,
                    StartDate = (DateTime)x.FStartDate,
                    EndDate = (DateTime)x.FEndDate,
                    Description = x.FDescription,
                    StatusId = (int)x.FStatusId,
                    StatusName = _db.TRequestStatuses
                                    .Where(s => s.FStatusId == x.FStatusId)
                                    .Select(s => s.FStatus).FirstOrDefault() ?? "",
                    PictureFileName = x.FPicture,
                    CreateTime = x.FCreatetime
                })
                .ToListAsync(ct);
        }

        //Deleted List
        public async Task<List<CLeaveItemDTO>> GetMyDeletedLeavesAsync(int employeeId, CancellationToken ct = default)
        {
            return await _db.TLeaves.AsNoTracking()
                .Where(x => x.FEmployeeId == employeeId && x.FStatusId == (int)LeaveRequestStatusEnum.Deleted)
                .OrderByDescending(x => x.FCreatetime)
                .Select(x => new CLeaveItemDTO
                {
                    LeaveId = x.FLeaveId,
                    EmployeeId = (int)x.FEmployeeId,
                    EmployeeName = _db.TEmployees
                                      .Where(e => e.FEmployeeId == x.FEmployeeId)
                                      .Select(e => e.FName).FirstOrDefault() ?? "",
                    LeaveType = x.FLeaveType,
                    StartDate = (DateTime)x.FStartDate,
                    EndDate = (DateTime)x.FEndDate,
                    Description = x.FDescription,
                    StatusId = (int)x.FStatusId,
                    StatusName = _db.TRequestStatuses
                                    .Where(s => s.FStatusId == x.FStatusId)
                                    .Select(s => s.FStatus).FirstOrDefault() ?? "",
                    PictureFileName = x.FPicture,
                    CreateTime = x.FCreatetime
                })
                .ToListAsync(ct);
        }

        //Create
        public async Task<int> CreateAsync(CLeaveCreateDTO dto, CancellationToken ct = default)
        {
            // 基本檢查
            if (dto.StartDate > dto.EndDate) throw new InvalidOperationException("開始時間不可晚於結束時間。");

            var entity = new TLeave
            {
                FEmployeeId = dto.EmployeeId,
                FLeaveType = dto.LeaveType,
                FPicture = dto.PictureFileName,
                FStartDate = dto.StartDate,
                FEndDate = dto.EndDate,
                FDescription = dto.Description,
                FStatusId = (int)LeaveRequestStatusEnum.Pending, // 預設待審
                FCreatetime = DateTime.Now
            };
            _db.TLeaves.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.FLeaveId;
        }

        // 主管審核List
        public async Task<List<CLeaveItemDTO>> GetPendingAsync(CancellationToken ct = default)
        {
            var pending = (int)LeaveRequestStatusEnum.Pending;
            return await _db.TLeaves.AsNoTracking()
                .Where(x => x.FStatusId == pending)
                .OrderBy(x => x.FCreatetime)
                .Select(x => new CLeaveItemDTO
                {
                    LeaveId = x.FLeaveId,
                    EmployeeId = (int)x.FEmployeeId,
                    EmployeeName = _db.TEmployees
                                      .Where(e => e.FEmployeeId == x.FEmployeeId)
                                      .Select(e => e.FName).FirstOrDefault() ?? "",
                    LeaveType = x.FLeaveType,
                    StartDate = (DateTime)x.FStartDate,
                    EndDate = (DateTime)x.FEndDate,
                    Description = x.FDescription,
                    StatusId = (int)x.FStatusId,
                    StatusName = "待審核",
                    PictureFileName = x.FPicture,
                    CreateTime = x.FCreatetime
                })
                .ToListAsync(ct);
        }

        //Approve
        public async Task<bool> ApproveAsync(int leaveId, int approverId, CancellationToken ct = default)
        {
            var affected = await _db.TLeaves
                .Where(l => l.FLeaveId == leaveId && l.FStatusId == (int)LeaveRequestStatusEnum.Pending)
                .ExecuteUpdateAsync(s => s.SetProperty(l => l.FStatusId, (int)LeaveRequestStatusEnum.Approved), ct);

            // 你可以另外記錄審核人、審核時間（需要加欄位再補）
            return affected > 0;
        }

        //Reject
        public async Task<bool> RejectAsync(int leaveId, int approverId, string? reason = null, CancellationToken ct = default)
        {
            var affected = await _db.TLeaves
                .Where(l => l.FLeaveId == leaveId && l.FStatusId == (int)LeaveRequestStatusEnum.Pending)
                .ExecuteUpdateAsync(s => s.SetProperty(l => l.FStatusId, (int)LeaveRequestStatusEnum.Rejected), ct);

            // reason 若要記錄，請在 tLeave 加欄位（例如 fRejectReason），然後一起 SetProperty
            return affected > 0;
        }

        //GetById
        public async Task<CLeaveItemDTO?> GetByIdAsync(int leaveId, CancellationToken ct = default)
        {
            return await _db.TLeaves.AsNoTracking()
                .Where(x => x.FLeaveId == leaveId)
                .Select(x => new CLeaveItemDTO
                {
                    LeaveId = x.FLeaveId,
                    EmployeeId = (int)x.FEmployeeId,
                    EmployeeName = _db.TEmployees
                                      .Where(e => e.FEmployeeId == x.FEmployeeId)
                                      .Select(e => e.FName).FirstOrDefault() ?? "",
                    LeaveType = x.FLeaveType,
                    StartDate = (DateTime)x.FStartDate,
                    EndDate = (DateTime)x.FEndDate,
                    Description = x.FDescription,
                    StatusId = (int)x.FStatusId,
                    StatusName = _db.TRequestStatuses
                                    .Where(s => s.FStatusId == x.FStatusId)
                                    .Select(s => s.FStatus).FirstOrDefault() ?? "",
                    PictureFileName = x.FPicture,
                    CreateTime = x.FCreatetime
                })
                .SingleOrDefaultAsync(ct);
        }

        //softDeleted
        public async Task<bool> SoftDeletedAsync(int leaveId, CancellationToken ct = default)
        {
            // 把非刪除狀態且為待審核改成 Deleted=4
            var affected = await _db.TLeaves
                .Where(l => l.FLeaveId == leaveId && l.FStatusId != (int)LeaveRequestStatusEnum.Deleted && l.FStatusId == (int)LeaveRequestStatusEnum.Pending)
                .ExecuteUpdateAsync(s => s.SetProperty(l => l.FStatusId, (int)LeaveRequestStatusEnum.Deleted), ct);

            return affected > 0;
        }

        //Delete
        public async Task<bool> DeleteLeavesAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0) return false;
            var delete = await _db.TLeaves
                .Where(e => e.FLeaveId == id && e.FStatusId == (int)LeaveRequestStatusEnum.Deleted)
                .ExecuteDeleteAsync(ct);

            return delete > 0;
        }
    }
}
