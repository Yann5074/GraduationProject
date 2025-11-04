using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;

namespace ApiProject.Services
{
    public class CAnnouncementService : IAnnouncementService
    {
        private readonly dbFurniMartContext _context;
        public CAnnouncementService(dbFurniMartContext context)
        {
            _context = context;
        }

        // 共用 mapping
        private static CAnnouncementDTO ToDto(TAnnouncement a)
        {
            var now = DateTime.UtcNow;
            return new CAnnouncementDTO
            {
                Id = a.FId,
                Title = a.FTitle ?? "",
                Message = a.FMessage ?? "",
                StartAt = a.FStartAt ?? now,
                EndAt = a.FEndAt,
                IsActive = a.FIsActive ?? true,
                Priority = a.FPriority ?? 0,
                LastUpdated = a.FLastUpdated ?? now
            };
        }

    }
}
