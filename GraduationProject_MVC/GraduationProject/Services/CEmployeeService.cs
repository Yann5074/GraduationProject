using GraduationProject.DTOs;
using GraduationProject.Enum;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    public class CEmployeeService : IEmployeeService
    {
        //建構子注入
        private readonly dbFurniMartContext _db;
        private readonly IPasswordHasher<TEmployee> _hasher;
        public CEmployeeService(dbFurniMartContext db, IPasswordHasher<TEmployee> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // 關鍵字搜尋方法
        private static IQueryable<TEmployee> KeywordFilter(IQueryable<TEmployee> q, string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return q;

            var pattern = $"%{keyword.Trim()}%";
            return q.Where(e =>
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

        //List
        public async Task<List<CEmployeeListItemDTO>> GetEmployeeListAsync(
            string? keyword, CancellationToken ct = default)
        {
            // 基底查詢：不含 Include
            IQueryable<TEmployee> q = _db.TEmployees
                .AsNoTracking()
                .Where(e => e.FStatusId != (int)CEmployeeStatusEnum.Deleted);//帳號註銷不顯示

            // 關鍵字方法
            q = KeywordFilter(q, keyword);

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

        //Deleted List
        public async Task<List<CEmployeeListItemDTO>> GetEmployeeDeletedListAsync(
           string? keyword, CancellationToken ct = default)
        {
            IQueryable<TEmployee> q = _db.TEmployees
               .AsNoTracking()
               .Where(e => e.FStatusId == (int)CEmployeeStatusEnum.Deleted);//只顯示註銷帳號

            // 關鍵字方法
            q = KeywordFilter(q, keyword);

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

        //Create
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

        //FakeDelete
        public async Task<bool> DeleteEmployeeAsync(int? id)
        {
            if (id is null || id <= 0) return false;

            var affected = await _db.TEmployees
                .Where(e => e.FEmployeeId == id.Value && e.FStatusId != 4)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.FStatusId, 4));

            return affected > 0;
        }

        //ReallyDelete
        public async Task<bool> ReallyDeleteEmployeeAsync(int? id)
        {
            if (id is null || id <= 0) return false;

            var delete = await _db.TEmployees
                .Where(e => e.FEmployeeId == id.Value && e.FStatusId == 4)
                .ExecuteDeleteAsync();

            return delete > 0;
        }

        //Edit畫面
        public async Task<bool> EditEmployeeAsync(int id, CEmployeeEditDTO dto, CancellationToken ct = default)
        {
            var emp = await _db.TEmployees.FirstOrDefaultAsync(x => x.FEmployeeId == id, ct);
            if (emp is null) return false;

            emp.FName = dto.FName;
            emp.FPhone = dto.FPhone;
            emp.FEmail = dto.FEmail;
            if (!string.IsNullOrWhiteSpace(dto.FHeadShot))
                emp.FHeadShot = dto.FHeadShot;
            emp.FGender = dto.FGender;
            emp.FBloodType = dto.FBloodType;
            emp.FHireDate = dto.FHireDate;
            emp.FRoleId = dto.FRoleId;
            emp.FStatusId = dto.FStatusId;
            // emp.FAccount = dto.FAccount;

            if (!string.IsNullOrWhiteSpace(dto.FPasswords))
            {
                emp.FPasswords = _hasher.HashPassword(emp, dto.FPasswords);
                emp.FChangePasswordTime = DateTime.Now;
            }

            if (dto.FLoginTime.HasValue)
                emp.FLoginTime = dto.FLoginTime;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        //讀取EditViewModel
        public async Task<CEmployeeEditViewModel?> GetEmployeeEditVmAsync(int id, CancellationToken ct = default)
        {
            return await _db.TEmployees
                .AsNoTracking()
                .Where(e => e.FEmployeeId == id)
                .Select(e => new CEmployeeEditViewModel
                {
                    FName = e.FName ?? string.Empty,
                    FPhone = e.FPhone ?? string.Empty,
                    FEmail = e.FEmail ?? string.Empty,
                    FHeadShot = e.FHeadShot,
                    FGender = e.FGender,
                    FBloodType = e.FBloodType ?? string.Empty,
                    FHireDate = e.FHireDate,
                    FRoleId = e.FRoleId,
                    FStatusId = e.FStatusId,
                    FAccount = e.FAccount,
                    FPasswords = string.Empty,
                    FLoginTime = e.FLoginTime,
                    FChangePasswordTime = e.FChangePasswordTime
                })
                .FirstOrDefaultAsync(ct);
        }

        //Details
        public async Task<CEmployeeDetailDTO?> DetailEmployeeAsync(int? id, bool fromDeleted = false, CancellationToken ct = default)
        {
            return await _db.TEmployees
            .AsNoTracking()
            .Where(e => e.FEmployeeId == id)
            .Select(e => new CEmployeeDetailDTO
                {
                    FEmployeeId = e.FEmployeeId,
                    FHeadShot = e.FHeadShot,

                    FName = e.FName,
                    FPhone = e.FPhone,
                    FEmail = e.FEmail,

                    FBloodType = e.FBloodType,
                    FHireDate = e.FHireDate,

                    FGender = e.FGender,
                    FGenderName = e.FGenderNavigation != null ? e.FGenderNavigation.FGenderName : null,

                    FRoleId = e.FRoleId,
                    FRoleClass = e.FRole != null ? e.FRole.FRoleClass : null,

                    FStatusId = e.FStatusId,
                    FStatus = e.FStatus != null ? e.FStatus.FStatus : null,

                    FAccount = e.FAccount,
                    FLoginTime = e.FLoginTime,
                    FChangePasswordTime = e.FChangePasswordTime
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
