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
        //填寫會員資料及可修改手機和Email
        public Task MemberUpdateMeAsync(int memberId, ReqMemberUpdateDTO req, CancellationToken ct = default);
        //登出
        public Task<ResultDTO> MemberLogoutAsync(CancellationToken ct = default);
        //修改密碼
        public Task<ResultDTO> MemberUpdatePasswordAsync(int memberId, ReqMemberUpdatePasswordDTO req, CancellationToken ct = default);

        //確認目前密碼
        public Task<bool> CheckPasswordAsync(int memberId, string rawPassword, CancellationToken ct = default);
        //產生 6 碼驗證碼
        public Task<ResultDTO> SendEmailVerificationCodeAsync(string email);
        //前端輸入驗證碼
        public Task<ResultDTO> VerifyEmailCodeAsync(string email, string code);

        //上傳大頭貼
        //public Task<ResMemberUploadPhotoDTO> MemberUploadPhotoAsync(int memberId, IFormFile file, CancellationToken ct = default);


    }
}
