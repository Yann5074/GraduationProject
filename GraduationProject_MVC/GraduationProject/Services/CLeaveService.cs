using GraduationProject.DTOs;
using GraduationProject.Enum;
using GraduationProject.Extensions;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    public class CLeaveService : ILeaveService
    {
        private readonly dbFurniMartContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CLeaveService> _logger;
        public CLeaveService(dbFurniMartContext db, IWebHostEnvironment env, ILogger<CLeaveService> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        //List
        public async Task<PagedList<CLeaveItemDTO>> GetMyLeavesAsync(int employeeId, string? keyword, DateTime? start, DateTime? end, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            IQueryable<TLeave> q = _db.TLeaves.AsNoTracking()
                    .Where(x => x.FEmployeeId == employeeId && x.FStatusId != (int)LeaveRequestStatusEnum.Deleted);

            // 關鍵字
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(x => (x.FDescription ?? "").Contains(keyword));

            // 時間區間（以 FStartDate 或 FCreatetime 篩選都可，選你要的欄位）
            if (start.HasValue)
            {
                var s = start.Value.Date;                 // 當天 00:00
                q = q.Where(x => x.FStartDate >= s);      // 起：含當天
            }
            if (end.HasValue)
            {
                var e = end.Value.Date.AddDays(1);        // 取「小於隔天 00:00」→ 等同「包含結束日整天」
                q = q.Where(x => x.FStartDate < e);       // 迄：不含隔天
            }

            // 投影 + 分頁（用你現成的 ToPagedListAsync 擴充方法）
            var list = q.OrderByDescending(x => x.FCreatetime)
                .Select(x => new CLeaveItemDTO
                {
                    LeaveId = x.FLeaveId,
                    LeaveType = x.FLeaveType!,
                    StartDate = x.FStartDate ?? DateTime.MinValue,
                    EndDate = x.FEndDate ?? DateTime.MinValue,
                    StatusId = x.FStatusId ?? 0,
                    StatusName = _db.TRequestStatuses
                                    .Where(s => s.FStatusId == x.FStatusId)
                                    .Select(s => s.FStatus)
                                    .FirstOrDefault() ?? "未知",
                    PictureFileName = x.FPicture,
                    CreateTime = x.FCreatetime
                });

            return await list.ToPagedListAsync(page, pageSize, ct);
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

            // 先查出狀態與檔名
            var info = await _db.TLeaves
                .Where(x => x.FLeaveId == id)
                .Select(x => new { x.FStatusId, x.FPicture })
                .SingleOrDefaultAsync(ct);

            if (info is null) return false;
            if (info.FStatusId != (int)LeaveRequestStatusEnum.Deleted) return false;

            var delete = await _db.TLeaves
                .Where(e => e.FLeaveId == id && e.FStatusId == (int)LeaveRequestStatusEnum.Deleted)
                .ExecuteDeleteAsync(ct);
            if (delete <= 0) return false;

            // 再清檔案
            if (!string.IsNullOrWhiteSpace(info.FPicture))
            {
                try
                {
                    // 取檔名
                    var fileName = Path.GetFileName(info.FPicture);
                    var fullPath = Path.Combine(_env.WebRootPath, "LeaveEvidence", fileName);
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "刪除證明檔失敗：{FPicture}", info.FPicture);
                }
            }

            return true;
        }
    }
}
