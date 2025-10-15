using ApiProject.DTOs;
using Microsoft.AspNetCore.Identity.Data;

namespace ApiProject.Interfaces
{
    public interface IMemberService
    {
        //註冊帳號
        public Task<ResultDTO> MemberCreateAccountAsync(ReqMemberCreateAccountDTO reqdto, CancellationToken ct = default);
        //登入
        public Task<ResMemberDTO?> MemberLoginAsync(ReqMemberLoginDTO reqdto, CancellationToken ct = default);
        //取得會員資料
        public Task<ResMemberDTO> GetMemberMeAsync(int memberId, CancellationToken ct = default);
        //填寫會員資料及可改手機和Email
        public Task MemberUpdateMeAsync(int memberId, ReqMemberUpdateDTO req, CancellationToken ct = default);
        //登出
        public Task<ResultDTO> MemberLogoutAsync(CancellationToken ct = default);
        //public Task ChangePasswordAsync(int memberId, ReqMemberChangePasswordDTO req);

        //public Task<string> SaveAvatarAsync(int memberId, IFormFile file);

    }
}
