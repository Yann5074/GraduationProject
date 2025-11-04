using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IAnnouncementService
    {
        Task<IReadOnlyList<CAnnouncementDTO>> GetAllAsync(bool? active, CancellationToken ct);
    }
}
