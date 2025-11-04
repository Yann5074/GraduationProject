using GraduationProject.DTOs;

namespace GraduationProject.Interfaces
{
    public interface IAnnouncementService
    {
        Task<List<CAnnouncementDTO>?> GetAllAsync(bool? active = null);
        Task<CAnnouncementDTO> GetAsync(int id);
        Task CreateAsync(CSaveAnnouncementDTO dto);
        Task UpdateAsync(int id, CSaveAnnouncementDTO dto);
        Task DeleteAsync(int id);
    }
}
