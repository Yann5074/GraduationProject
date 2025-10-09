using GraduationProject.DTOs;
//using ApiProject.Models;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Humanizer;

// using Microsoft.CodeAnalysis.Elfie.Model; // 移除
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    public class CMemberService : IMemberService
    {
        private readonly dbFurniMartContext _db;
        private  IWebHostEnvironment _enviro;
        public CMemberService(dbFurniMartContext db, IWebHostEnvironment enviro)
        {
            _db = db;
            _enviro = enviro;
        } 

        public async Task<CMemberDTO?> GetAsync(int memberId, CancellationToken ct = default)
        {
            var p = await _db.TMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FMemberId == memberId, ct);

            return p == null ? null : MapToDto(p);
        }

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

        //public bool MemberUpdate(CMemberUpdateDTO dtoui, CancellationToken ct = default)
        //{
        //    TMember od = _db.TMembers.FirstOrDefault(o => o.FMemberId == dtoui.MemberId);
        //    if (od == null)
        //        return false;

        //od.FName = dtoui.Name;
        //    od.FDisplayName = dtoui.DisplayName;
        //    od.FPhone = dtoui.Phone;
        //    od.FAddress = dtoui.Address;
        //    od.FGender = dtoui.Gender;
        //    od.FLeveId = dtoui.LeveId;
        //    od.FMoneySum = dtoui.MoneySum;
        //    od.FUpdateTime = DateTime.Now;
        //    od.FCreatTime = dtoui.CreatTime;
        //    od.FStatus = dtoui.Status;

        //    // 只有在 dto.MemberImage 有值時才更新，避免覆蓋成 null
        //    if (!string.IsNullOrWhiteSpace(dtoui.MemberImage))
        //        od.FMemberImage = dtoui.MemberImage;

        //    _db.SaveChanges();
        //    return true;
        //}


        //public CMemberUpdateDTO SearchUpdateMember(int? id)
        //{
        //    var dto = new CMemberUpdateDTO()
        //    {
        //        isValid = false
        //    };

        //    if (id == null)
        //        return dto;

        //    TMember od = _db.TMembers.FirstOrDefault(o => o.FMemberId == id);
        //    if (od == null)
        //        return dto;
        //dto.isValid = true;
        //    dto.MemberId = od.FMemberId;
        //    dto.Name = od.FName;
        //    dto.DisplayName = od.FDisplayName;
        //    dto.Gender = od.FGender;
        //    dto.Phone = od.FPhone;
        //    dto.Address = od.FAddress;
        //    dto.MemberImage = od.FMemberImage;
        //    dto.LeveId = od.FLeveId;
        //    dto.MoneySum = od.FMoneySum;
        //    dto.Status = od.FStatus;
        //    dto.CreatTime = od.FCreatTime;
        //    dto.Account = od.FAccount;
        //    dto.Passwords = od.FPasswords;

        //    return dto;
        //}
        private static int GetLevelByMoney(int money)
        {
            if (money >= 250_000) return 4;  // 白金
            if (money >= 100_000) return 3;  // 金
            if (money >= 10_000) return 2;  // 銀
            return 1;                        // 銅
        }
        public async Task<bool> MemberEdit(int id, CMemberUpdateDTO dto, CancellationToken ct = default)
        {
            var mem = await _db.TMembers.FirstOrDefaultAsync(o => o.FMemberId == id,ct);
            if(mem == null)return false;
            mem.FName = dto.Name;
            mem.FDisplayName = dto.DisplayName;
            mem.FPhone = dto.Phone;
            mem.FAddress = dto.Address;
            mem.FGender = dto.Gender;

            // 以「使用者送來的金額」為準，回推等級
            var money = dto.MoneySum ?? 0;
            mem.FMoneySum = money;
            mem.FLeveId = GetLevelByMoney(money);

            mem.FUpdateTime = DateTime.Now;
            mem.FCreatTime = dto.CreatTime;
            mem.FStatus = dto.Status;
            if (!string.IsNullOrWhiteSpace(dto.MemberImage))
                mem.FMemberImage = dto.MemberImage;

            //if (!string.IsNullOrWhiteSpace(dto.Passwords))
            //{
            //    mem.FPasswords = dto.Passwords;
            //}

            await _db.SaveChangesAsync(ct);
            return true;
            
        }
        public async Task<CMemberUpdateViewModel?> GetEditMember(int id, CancellationToken ct = default)
        {
            return await _db.TMembers
                .AsNoTracking()
                .Where(e => e.FMemberId == id)
                .Select(e => new CMemberUpdateViewModel
                {
                    Name = e.FName ?? string.Empty,
                    DisplayName = e.FDisplayName ?? string.Empty,
                    Gender = e.FGender,
                    Phone = e.FPhone ?? string.Empty,
                    Address = e.FAddress ?? string.Empty,
                    MemberImage = e.FMemberImage,
                    LeveId = e.FLeveId,
                    MoneySum = e.FMoneySum,
                    Status = e.FStatus,
                    CreatTime = e.FCreatTime,
                    Account = e.FAccount ?? string.Empty,
                    //Passwords = e.FPasswords ?? string.Empty
                })
                .FirstOrDefaultAsync(ct);
        }


        //public CMemberUpdateDTO SearchUpdateMember(int? id)
        //{
        //    var dto = new CMemberUpdateDTO()
        //    {
        //        isValid = false
        //    };

        public async Task<CMemberDetailsDTO?> GetMemberDetailsAsync(int id, CancellationToken ct = default)
        {
            var e = await _db.TMembers
               .AsNoTracking()
               .Include(x => x.FGenderNavigation)
               .Include(x => x.FStatusNavigation)
               .Include(x => x.FLeveIdNavigation)
               .FirstOrDefaultAsync(x => x.FMemberId == id, ct);

            if (e == null) return null;

            return new CMemberDetailsDTO
            {
                MemberId = e.FMemberId,
                Name = e.FName,
                DisplayName = e.FDisplayName,
                Gender = e.FGender,
                GenderName = e.FGenderNavigation?.FGenderName,
                Phone = e.FPhone,
                Address = e.FAddress,
                Status = e.FStatus,
                MoneySum= e.FMoneySum,
                StatusName = e.FStatusNavigation?.FStatusName,
                LeveId = e.FLeveId,
                LevelName = e.FLeveIdNavigation?.FLevelName,
                MemberImage = e.FMemberImage,
                CreatTime = e.FCreatTime,
                UpdateTime = e.FUpdateTime
            };

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
