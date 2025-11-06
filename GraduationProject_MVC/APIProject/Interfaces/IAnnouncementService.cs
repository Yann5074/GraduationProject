using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IAnnouncementService
    {
        Task<IReadOnlyList<CAnnouncementDTO>> GetAllAsync(bool? active, CancellationToken ct);
        Task<IReadOnlyList<CAnnouncementDTO>> GetActiveAsync(CancellationToken ct);
        Task<CAnnouncementDTO?> GetAsync(int id, CancellationToken ct);
        Task<CAnnouncementDTO> CreateAsync(CSaveAnnouncementDTO dto, CancellationToken ct);
    }
}
