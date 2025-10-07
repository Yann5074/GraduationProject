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
                .OrderBy(x => x.FMemberId)
                .Select(x => new CMemberDTO
                {
                    MemberId = x.FMemberId,
                    Name = x.FName,
                    Phone = x.FPhone,
                    DisplayName = x.FDisplayName,
                    Address = x.FAddress,
                    Gender = x.FGender,
                    Status = x.FStatus,
                    LeveId = x.FLeveId

                }).ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CMemberDTO>> MemberSearchAsync(string? keyword, CancellationToken ct = default)
        {
            var query = _db.TMembers.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(p =>
                    (p.FName ?? "").Contains(kw)||
                    (p.FPhone ?? "").Contains(kw) ||
                    (p.FAddress ?? "").Contains(kw));
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
                    Gender = p.FGender,
                    Status = p.FStatus,
                    LeveId = p.FLeveId
                })
                .ToListAsync(ct);
        }


        private static CMemberDTO MapToDto(TMember x) => new CMemberDTO
        {
            MemberId = x.FMemberId,
            Name = x.FName,
            Phone = x.FPhone,
            DisplayName = x.FDisplayName,
            Address = x.FAddress,
            Gender = x.FGender,
            Status = x.FStatus,
            LeveId = x.FLeveId
        };

    }
}
