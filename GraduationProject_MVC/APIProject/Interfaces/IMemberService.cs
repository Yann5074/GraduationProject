using ApiProject.DTOs;
using Microsoft.AspNetCore.Identity.Data;

namespace ApiProject.Interfaces
{
    public interface IMemberService
    {
        public Task<ResultDTO> MemberCreateAsync(ReqMemberCreateDTO reqdto, CancellationToken ct = default);
        //public Task<ResMemberDTO?> ValidateUserAsync(string account, string password);
        //public Task<ResMemberDTO> GetMeAsync(int memberId);
        //public Task UpdateMeAsync(int memberId, ReqMemberUpdateDTO req);
        //public Task ChangePasswordAsync(int memberId, ReqMemberChangePasswordDTO req);
        //public Task<string> SaveAvatarAsync(int memberId, IFormFile file);

    }
}
