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

        //產生一次性 Token（重設密碼用）
        public async Task<string?> GenerateResetTokenAsync(string account, string email, CancellationToken ct = default)
        {
            var user = await _db.TEmployees
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.FAccount == account && e.FEmail == email, ct);

            if (user is null) return null;

            // 清掉舊的未使用 Token
            await _db.TEmployeePasswordResets
                     .Where(r => r.FEmployeeId == user.FEmployeeId && r.FUsedAt == null && r.FExpiresAt > DateTime.Now)
                     .ExecuteDeleteAsync(ct);

            //產生新 Token
            var token = Guid.NewGuid().ToString("N");//純 32 字元識別碼

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

            // 找 token
            var reset = await _db.TEmployeePasswordResets
                .SingleOrDefaultAsync(r =>
                     r.FToken == token && //Token 必須一致
                     r.FEmployeeId == user.FEmployeeId && //對應到同一個人
                     r.FUsedAt == null && //未使用
                     r.FExpiresAt > DateTime.Now, ct); //未過期
            if (reset is null) return false; //無效或過期的連結

            // 密碼強度
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
            //不允許空白, 長度需>8
            if (string.IsNullOrWhiteSpace(pwd) || pwd.Length < 8) return false;

            string allowed = "!@#$%^&*()-_=+[]{};:'\",.<>/?\\|`~";
            bool lower = pwd.Any(char.IsLower), //小寫
                 upper = pwd.Any(char.IsUpper), //大寫
                 digit = pwd.Any(char.IsDigit), //數字
                 sym   = pwd.Any(allowed.Contains); //符號
            return lower && upper && digit && sym;
        }
    }
}
