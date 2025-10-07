using GraduationProject.DTOs;

namespace GraduationProject.Interfaces
{
    public interface IMemberService
    {
        Task <IReadOnlyList<CMemberDTO>> MemberListAsync(CancellationToken ct = default);

        Task<CMemberDTO?> GetAsync(int productId, CancellationToken ct = default);

        Task<IReadOnlyList<CMemberDTO>> MemberSearchAsync(string? keyword, CancellationToken ct = default);
    }
}
