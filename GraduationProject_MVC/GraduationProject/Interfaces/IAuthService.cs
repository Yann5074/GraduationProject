using GraduationProject.DTOs;

namespace GraduationProject.Interfaces
{
    public interface IAuthService
    {
        //Login
        Task<CAuthResultDTO> AuthenticateAsync(string account, string password, CancellationToken ct = default);

        //forget passwords setting
        Task<string?> GenerateResetTokenAsync(string account, string email, CancellationToken ct = default);
        Task<bool> ResetPasswordAsync(string account, string token, string newPassword, CancellationToken ct = default);

    }
}
