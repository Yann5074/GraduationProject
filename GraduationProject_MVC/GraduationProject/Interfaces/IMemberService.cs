using GraduationProject.DTOs;
using GraduationProject.ViewModels;

namespace GraduationProject.Interfaces
{
    public interface IMemberService
    {
        Task <IReadOnlyList<CMemberDTO>> MemberListAsync(CancellationToken ct = default);

        Task<CMemberDTO?> GetAsync(int memberId, CancellationToken ct = default);

        Task<IReadOnlyList<CMemberDTO>> MemberSearchAsync(string? keyword, CancellationToken ct = default);

        Task<CMemberCreatedDTO> MemberCreateAsync(CMemberCreateDTO dto, CancellationToken ct = default);

        public bool MemberDelete(int? id);

        //Update 更新訂單
        Task <bool> MemberEdit(int id,CMemberUpdateDTO dto, CancellationToken ct = default);

        //SearchUpdate 搜尋欲更新訂單
        Task<CMemberUpdateViewModel?> GetEditMember(int id, CancellationToken ct = default);

        Task<CMemberDetailsDTO?> GetMemberDetailsAsync(int id, CancellationToken ct = default);
    }
}
