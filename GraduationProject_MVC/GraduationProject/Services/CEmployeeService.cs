using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    public class CEmployeeService : IEmployeeService
    {
        //私有唯讀欄位 _db，型別是你的 EF Core DbContext。
        private readonly dbFurniMartContext _db;
        //建構子注入：DI 會把已註冊的 dbFurniMartContext 傳進來
        public CEmployeeService(dbFurniMartContext db) => _db = db;

        //對外公開方法，非同步回傳 Task<List<CEmployeeListItemDTO>>（清單 DTO）。
        //ct 是 取消權杖，預設值 default 代表呼叫端可選擇性傳入，非必要。
        public async Task<List<CEmployeeListItemDTO>> GetEmployeeListAsync(
            string? keyword, CancellationToken ct = default)
        {
            // 基底查詢：不含 Include
            IQueryable<TEmployee> q = _db.TEmployees.AsNoTracking();

            // 關鍵字條件（可被翻譯成 SQL，含導覽欄位的篩選）
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var pattern = $"%{keyword.Trim()}%";
                q = q.Where(e =>
                    EF.Functions.Like(e.FName ?? "", pattern) ||
                    EF.Functions.Like(e.FPhone ?? "", pattern) ||
                    EF.Functions.Like(e.FEmail ?? "", pattern) ||
                    EF.Functions.Like(e.FBloodType ?? "", pattern) ||
                    EF.Functions.Like(e.FAccount ?? "", pattern)
                    //(e.FGenderNavigation != null && EF.Functions.Like(e.FGenderNavigation.FGenderName ?? "", pattern)) ||
                    //(e.FRole != null && EF.Functions.Like(e.FRole.FRoleClass ?? "", pattern)) ||
                    //(e.FStatus != null && EF.Functions.Like(e.FStatus.FStatus ?? "", pattern))
                );
            }

            return await q.OrderBy(e => e.FEmployeeId)
                          .Select(e => new CEmployeeListItemDTO
                          {
                              Id = e.FEmployeeId,
                              Name = e.FName,
                              Phone = e.FPhone,
                              Email = e.FEmail,
                              BloodType = e.FBloodType,
                              Account = e.FAccount,
                              //GenderName = e.FGenderNavigation != null ? e.FGenderNavigation.FGenderName : null,
                              //RoleClass = e.FRole != null ? e.FRole.FRoleClass : null,
                              //Status = e.FStatus != null ? e.FStatus.FStatus : null
                          })
                          .ToListAsync(ct);
        }
    }
}
