using GraduationProject.DTOs;

namespace GraduationProject.Interfaces
{
    public interface IAuthService
    {
        Task<CAuthResultDTO> AuthenticateAsync(string account, string password, CancellationToken ct = default);
    }
}
