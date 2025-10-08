using GraduationProject.DTOs;
//using ApiProject.Models;
using GraduationProject.Interfaces;
using GraduationProject.Models;
// using Microsoft.CodeAnalysis.Elfie.Model; // 移除
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    public class CMemberService : IMemberService
    {
        private readonly dbFurniMartContext _db;
        public CMemberService(dbFurniMartContext db) => _db = db;

        public async Task<CMemberDTO?> GetAsync(int memberId, CancellationToken ct = default)
        {
            var p = await _db.TMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FMemberId == memberId, ct);

            return p == null ? null : MapToDto(p);
        }

        public async Task<IReadOnlyList<CMemberDTO>> MemberListAsync(CancellationToken ct = default)
        {
            return await _db.TMembers
            .AsNoTracking()
            .Include(x => x.FGenderNavigation)
            .Include(x => x.FStatusNavigation)
            .Include(x => x.FLeveIdNavigation)
            .OrderBy(x => x.FMemberId)
            .Select(x => new CMemberDTO
            {
                MemberId = x.FMemberId,
                Name = x.FName,
                Phone = x.FPhone,
                DisplayName = x.FDisplayName,
                Address = x.FAddress,
                MemberImage = x.FMemberImage,


                // 關聯表顯示名稱
                GenderName = x.FGenderNavigation != null ? x.FGenderNavigation.FGenderName : null,
                StatusName = x.FStatusNavigation != null ? x.FStatusNavigation.FStatusName : null,
                LevelName = x.FLeveIdNavigation != null ? x.FLeveIdNavigation.FLevelName : null
             })
            .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CMemberDTO>> MemberSearchAsync(string? keyword, CancellationToken ct = default)
        {
            var query = _db.TMembers
                .AsNoTracking()
                .Include(p => p.FGenderNavigation)
                .Include(p => p.FStatusNavigation)
                .Include(p => p.FLeveIdNavigation).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // (可選) 先把 LIKE 的特殊字元處理一下，避免誤判
                static string EscapeLike(string s)
                    => s.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

                var raw = EscapeLike(keyword.Trim());

                // ★ 若資料庫是大小寫不敏感（CI）照下行寫即可
                var k = $"%{raw}%";

                query = query.Where(p =>
                    EF.Functions.Like(p.FName ?? "", k) ||
                    EF.Functions.Like(p.FPhone ?? "", k) ||
                    EF.Functions.Like(p.FAddress ?? "", k) ||
                    EF.Functions.Like(p.FDisplayName ?? "", k) ||
                    (p.FGenderNavigation != null && EF.Functions.Like(p.FGenderNavigation.FGenderName ?? "", k)) ||
                    (p.FStatusNavigation != null && EF.Functions.Like(p.FStatusNavigation.FStatusName ?? "", k)) ||
                    (p.FLeveIdNavigation != null && EF.Functions.Like(p.FLeveIdNavigation.FLevelName ?? "", k))
                );
            }

            return await query
                .OrderBy(p => p.FMemberId)
                .Select(p => new CMemberDTO
                {
                    MemberId = p.FMemberId,
                    Name = p.FName,
                    Phone = p.FPhone,
                    DisplayName = p.FDisplayName,
                    Address = p.FAddress,
                    MemberImage = p.FMemberImage,
                    // 顯示關聯名稱
                    GenderName = p.FGenderNavigation != null ? p.FGenderNavigation.FGenderName : null,
                    StatusName = p.FStatusNavigation != null ? p.FStatusNavigation.FStatusName : null,
                    LevelName = p.FLeveIdNavigation != null ? p.FLeveIdNavigation.FLevelName : null
                })
                .ToListAsync(ct);
        }


        public async Task<CMemberCreatedDTO> MemberCreateAsync(CMemberCreateDTO dto, CancellationToken ct = default)
        {
            // 例：手機不可重複（依需求可移除）
            if (!string.IsNullOrWhiteSpace(dto.FPhone))
            {
                bool exists = await _db.TMembers
                    .AsNoTracking()
                    .AnyAsync(m => m.FPhone == dto.FPhone, ct);
                if (exists)
                    throw new InvalidOperationException("此手機號碼已存在。");
            }

            var now = DateTime.Now; // 若要 UTC 改成 DateTime.UtcNow
            var entity = new TMember
            {
                FName = dto.FName,
                FDisplayName = dto.FDisplayName,
                FGender = dto.FGender,
                FPhone = dto.FPhone,
                //FBirthDate = dto.FBirthDate,   // 若資料表沒有此欄位就刪掉
                FAddress = dto.FAddress,

                // 預設值放在 service
                FMemberImage = "default.png",
                FLeveId = 1,
                FMoneySum = 0,
                FStatus = 1,
                FCreatTime = now,
                FUpdateTime = now
            };

            _db.TMembers.Add(entity);
            await _db.SaveChangesAsync(ct);

            return new CMemberCreatedDTO
            {
                FMemberId = entity.FMemberId,
                FName = entity.FName!,
                FDisplayName = entity.FDisplayName
            };
        }

        public bool MemberDelete(int? id)
        {
            if (id == null)
                return false;
            TMember od = _db.TMembers.FirstOrDefault(o => o.FMemberId == id);
            if (od == null)
                return false;
            //_context.TOrders.Remove(od); 硬刪語法
            od.FStatus = 3; //軟刪 
            od.FUpdateTime = DateTime.Now;
            _db.SaveChanges();
            return true;
        }




        private static CMemberDTO MapToDto(TMember x) => new CMemberDTO
        {
            MemberId = x.FMemberId,
            Name = x.FName,
            Phone = x.FPhone,
            DisplayName = x.FDisplayName,
            Address = x.FAddress,
            MemberImage = x.FMemberImage,
            Gender = x.FGender,
            Status = x.FStatus,
            LeveId = x.FLeveId,

        };



    }
}
