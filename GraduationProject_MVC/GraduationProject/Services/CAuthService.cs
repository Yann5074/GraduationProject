using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    public class CAuthService : IAuthService
    {
        private readonly dbFurniMartContext _db;
        private readonly IPasswordHasher<TEmployee> _hasher;

        public CAuthService(dbFurniMartContext db, IPasswordHasher<TEmployee> hasher)
        {
            _db = db;
            _hasher = hasher;
        }
        public async Task<CAuthResultDTO> AuthenticateAsync(string account, string password, CancellationToken ct = default)
        {
            // 1) 找帳號
            var user = await _db.TEmployees
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.FAccount == account, ct);

            if (user is null)
                return new CAuthResultDTO { Success = false, Error = "帳號或密碼有誤" };

            // 2) 驗證雜湊密碼
            var verify = _hasher.VerifyHashedPassword(user, user.FPasswords, password);
            if (verify == PasswordVerificationResult.Failed)
                return new CAuthResultDTO { Success = false, Error = "帳號或密碼有誤" };

            // 3) 需要 rehash → 以追蹤實體更新密碼
            if (verify == PasswordVerificationResult.SuccessRehashNeeded)
            {
                var tracked = await _db.TEmployees
                    .FirstAsync(e => e.FEmployeeId == user.FEmployeeId, ct);

                tracked.FPasswords = _hasher.HashPassword(tracked, password);
                tracked.FChangePasswordTime = DateTime.Now;
                await _db.SaveChangesAsync(ct);
            }

            // 4) 回傳登入成功的 session 資料
            return new CAuthResultDTO
            {
                Success = true,
                User = new SessionUser
                {
                    Id = user.FEmployeeId,
                    Name = user.FName,
                    HeadShot = user.FHeadShot,
                    Account = user.FAccount!,
                    Email = user.FEmail,
                    RoleId = user.FRoleId
                }
            };
        }
    }
}
