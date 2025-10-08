using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    public class CEmployeeService : IEmployeeService
    {
        private readonly dbFurniMartContext _db;
        private readonly IPasswordHasher<TEmployee> _hasher;
        public CEmployeeService(dbFurniMartContext db, IPasswordHasher<TEmployee> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

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
                    EF.Functions.Like(e.FAccount ?? "", pattern) ||
                    (e.FGenderNavigation != null && EF.Functions.Like(e.FGenderNavigation.FGenderName ?? "", pattern)) ||
                    (e.FRole != null && EF.Functions.Like(e.FRole.FRoleClass ?? "", pattern)) ||
                    (e.FStatus != null && EF.Functions.Like(e.FStatus.FStatus ?? "", pattern))
                );
            }

            return await q.OrderBy(e => e.FEmployeeId)
                          .Select(e => new CEmployeeListItemDTO
                          {
                              Id = e.FEmployeeId,
                              HeadShot = e.FHeadShot,
                              Name = e.FName,
                              Phone = e.FPhone,
                              Email = e.FEmail,
                              BloodType = e.FBloodType,
                              Account = e.FAccount,
                              GenderName = e.FGenderNavigation != null ? e.FGenderNavigation.FGenderName : null,
                              RoleClass = e.FRole != null ? e.FRole.FRoleClass : null,
                              Status = e.FStatus != null ? e.FStatus.FStatus : null
                          })
                          .ToListAsync(ct);
        }
        public async Task<int> CreateEmployeeAsync(CEmployeeCreateDTO dto, CancellationToken ct = default)
        {
            // 帳號重複檢查
            bool exists = await _db.TEmployees.AsNoTracking()
                               .AnyAsync(x => x.FAccount == dto.FAccount, ct);
            if (exists)
                throw new InvalidOperationException("帳號已存在。");

            var emp = new TEmployee
            {
                FName = dto.FName,
                FPhone = dto.FPhone,
                FEmail = dto.FEmail,
                FHeadShot = dto.HeadShotFileName ?? "default.png",
                FGender = dto.FGender,
                FBloodType = dto.FBloodType,
                FHireDate = dto.FHireDate,
                FRoleId = dto.FRoleId,
                FStatusId = dto.FStatusId,
                FAccount = dto.FAccount,
                FPasswords = dto.FPasswords,
                FLoginTime = DateTime.Now,
                FChangePasswordTime = DateTime.Now
            };

            // 雜湊密碼（使用 Identity 提供的 PasswordHasher）
            emp.FPasswords = _hasher.HashPassword(emp, dto.FPasswords);

            _db.TEmployees.Add(emp);
            await _db.SaveChangesAsync(ct);

            return emp.FEmployeeId;
        }
    }
}
