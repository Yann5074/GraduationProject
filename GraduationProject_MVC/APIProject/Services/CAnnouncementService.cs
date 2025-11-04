using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IReadOnlyList<CAnnouncementDTO>> GetAllAsync(bool? active, CancellationToken ct)
        {
            var q = _context.TAnnouncements.AsNoTracking();

            if (active.HasValue)
                q = q.Where(x => (x.FIsActive ?? false) == active.Value);

            var list = await q.OrderByDescending(x => x.FLastUpdated ?? DateTime.MinValue)
                              .ToListAsync(ct);

            return list.Select(ToDto).ToList();
        }
    }
}
