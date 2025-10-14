using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Services
{
    public class CMemberServices:IMemberService
    {
        private readonly dbFurniMartContext _context;
        private readonly IPasswordHasher<TMember> _hasher;
        public CMemberServices(dbFurniMartContext context, IPasswordHasher<TMember> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public async Task<ResultDTO> MemberCreateAsync(ReqMemberCreateDTO reqdto, CancellationToken ct = default)
        {
            // 手機號碼格式檢查（需為 10 碼數字）
            if (!string.IsNullOrWhiteSpace(reqdto.Phone))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(reqdto.Phone, @"^\d{10}$"))
                {
                    throw new InvalidOperationException("手機號碼格式不正確，需為10位數字。");
                }

                // 手機不可重複
                bool exists = await _context.TMembers
                    .AsNoTracking()
                    .AnyAsync(m => m.FPhone == reqdto.Phone, ct);

                if (exists)
                    throw new InvalidOperationException("此手機號碼已存在，請重新輸入");
            }

            // 帳號不可重複（依需求可移除）
            if (!string.IsNullOrWhiteSpace(reqdto.Account))
            {
                bool exists = await _context.TMembers
                    .AsNoTracking()
                    .AnyAsync(m => m.FAccount == reqdto.Account, ct);
                if (exists)
                    throw new InvalidOperationException("此帳號已存在，請重新輸入");
            }

            var entity = new TMember
            {
                FName = reqdto.Name,
                FDisplayName = reqdto.DisplayName,
                FGender = reqdto.Gender,
                FPhone = reqdto.Phone,
                FAddress = reqdto.Address,
                FAccount = reqdto.Account,              
                // 預設值放在 service
                FMemberImage = "default.png",
                FLeveId = 1,
                FMoneySum = 0,
                FStatus = 1,
                FCreatTime = DateTime.Now,
                FUpdateTime = DateTime.Now
            };

            // 密碼加鹽
            entity.FPasswords = _hasher.HashPassword(entity, reqdto.Password);

            _context.TMembers.Add(entity);
            await _context.SaveChangesAsync(ct);

            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK
            };
        }

        public async Task<ResultDTO> MemberLoginAsync(ReqMemberLoginDTO reqdto, CancellationToken ct = default)
        {
            // 1 檢查帳號是否存在
            var member = await _context.TMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.FAccount == reqdto.Account, ct);

            if (member == null)
                throw new InvalidOperationException("帳號不存在，請重新輸入");

            // 2️ 驗證密碼（比對雜湊）
            var result = _hasher.VerifyHashedPassword(member, member.FPasswords, reqdto.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new InvalidOperationException("密碼錯誤，請重新輸入");

            // 3️ 若演算法升級，重新雜湊（可選）
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                var tracked = await _context.TMembers.FirstAsync(m => m.FMemberId == member.FMemberId, ct);
                tracked.FPasswords = _hasher.HashPassword(tracked, reqdto.Password);
                await _context.SaveChangesAsync(ct);
            }

            // 4️ 回傳結果
            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
            };
        }



    }
}
