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
            var now = DateTime.Now;
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

            var list = await q.OrderByDescending(x => x.FLastUpdated ?? DateTime.MinValue).ToListAsync(ct);

            return list.Select(ToDto).ToList();
        }

        public async Task<IReadOnlyList<CAnnouncementDTO>> GetActiveAsync(CancellationToken ct)
        {
            var now = DateTime.Now;
            var list = await _context.TAnnouncements.AsNoTracking()
                .Where(a => (a.FIsActive ?? false) &&
                            (a.FStartAt ?? now) <= now &&
                            (a.FEndAt == null || a.FEndAt >= now))
                .OrderByDescending(a => a.FPriority ?? 0)
                .ThenByDescending(a => a.FStartAt ?? DateTime.MinValue)
                .ToListAsync(ct);

            return list.Select(ToDto).ToList();
        }

        public async Task<CAnnouncementDTO?> GetAsync(int id, CancellationToken ct)
        {
            var a = await _context.TAnnouncements.AsNoTracking()
                                           .FirstOrDefaultAsync(x => x.FId == id, ct);
            return a == null ? null : ToDto(a);
        }

        public async Task<CAnnouncementDTO> CreateAsync(CSaveAnnouncementDTO dto, CancellationToken ct)
        {
            if (dto.EndAt.HasValue && dto.EndAt.Value < dto.StartAt)
                throw new ArgumentException("結束時間不可早於開始時間");

            var e = new TAnnouncement
            {
                FTitle = dto.Title,
                FMessage = dto.Message,
                FStartAt = dto.StartAt,
                FEndAt = dto.EndAt,
                FIsActive = dto.IsActive,
                FPriority = dto.Priority,
                FLastUpdated = DateTime.Now
                // LastUpdated 由 DbContext.SaveChanges 補
            };

            _context.TAnnouncements.Add(e);
            await _context.SaveChangesAsync(ct);
            return ToDto(e);
        }
    }
}
