using ApiProject.DTOs;
using ApiProject.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;
        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        // GET: /api/announcements?active=true|false
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CAnnouncementDTO>>> GetAll([FromQuery] bool? active, CancellationToken ct)
        {
            var announcement = await _announcementService.GetAllAsync(active, ct);

            if (!announcement.Any())
                return NotFound("沒有公告");

            return Ok(announcement);
        }
    }
}
