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

        //登入
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

            // 3) rehash
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
                    RoleId = user.FRoleId,
                    StatusId = user.FStatusId
                }
            };
        }

        //重設密碼 產生一次性 Token（1 小時有效）
        public async Task<string?> GenerateResetTokenAsync(string account, string email, CancellationToken ct = default)
        {
            var user = await _db.TEmployees
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.FAccount == account && e.FEmail == email, ct);

            if (user is null) return null;

            // 你可以選擇：清掉該使用者「未用且未過期」的舊 token（避免多條可用連結）
            await _db.TEmployeePasswordResets
                     .Where(r => r.FEmployeeId == user.FEmployeeId && r.FUsedAt == null && r.FExpiresAt > DateTime.Now)
                     .ExecuteDeleteAsync(ct);

            var token = Guid.NewGuid().ToString("N");

            var reset = new TEmployeePasswordReset
            {
                FEmployeeId = user.FEmployeeId,
                FToken = token,
                FExpiresAt = DateTime.Now.AddHours(1),
                FCreatedAt = DateTime.Now
            };
            _db.TEmployeePasswordResets.Add(reset);
            await _db.SaveChangesAsync(ct);

            return token;
        }

        // 驗證 Token + 重設密碼（一次性）
        public async Task<bool> ResetPasswordAsync(string account, string token, string newPassword, CancellationToken ct = default)
        {
            // 找帳號
            var user = await _db.TEmployees.SingleOrDefaultAsync(e => e.FAccount == account, ct);
            if (user is null) return false;

            // 找 token（未使用、未過期、對應到同一個人）
            var reset = await _db.TEmployeePasswordResets
                .SingleOrDefaultAsync(r =>
                     r.FToken == token &&
                     r.FEmployeeId == user.FEmployeeId &&
                     r.FUsedAt == null &&
                     r.FExpiresAt > DateTime.Now, ct);
            if (reset is null) return false;

            // 密碼強度（可換 Regex 版本）
            if (!IsStrongPassword(newPassword)) return false;

            // 雜湊新密碼 + 設定使用時間
            user.FPasswords = _hasher.HashPassword(user, newPassword);
            user.FChangePasswordTime = DateTime.Now;
            reset.FUsedAt = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        // 強度檢查
        private static bool IsStrongPassword(string pwd)
        {
            if (string.IsNullOrWhiteSpace(pwd) || pwd.Length < 8) return false;
            bool lower = pwd.Any(char.IsLower), upper = pwd.Any(char.IsUpper),
                 digit = pwd.Any(char.IsDigit), sym = pwd.Any(ch => !char.IsLetterOrDigit(ch));
            return lower && upper && digit && sym;
        }
    }
}
