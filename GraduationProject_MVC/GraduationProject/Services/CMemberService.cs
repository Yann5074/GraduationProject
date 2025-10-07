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


    }
}
