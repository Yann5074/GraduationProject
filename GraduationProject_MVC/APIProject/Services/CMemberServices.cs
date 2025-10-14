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


    }
}
