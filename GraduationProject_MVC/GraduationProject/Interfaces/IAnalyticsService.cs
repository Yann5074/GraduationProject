using GraduationProject.DTOs;

namespace GraduationProject.Interfaces
{
    public interface IAnalyticsService
    {
        Task<COrderDashboardDTO> GetDashboardAsync(CancellationToken ct = default);
    }
}
