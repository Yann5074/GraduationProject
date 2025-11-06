using ApiProject.DTOs;
using ApiProject.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

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

        // GET: /api/announcement?active=true|false
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CAnnouncementDTO>>> GetAll([FromQuery] bool? active, CancellationToken ct)
        {
            var announcement = await _announcementService.GetAllAsync(active, ct);

            if (!announcement.Any())
                return NotFound("沒有公告");

            return Ok(announcement);
        }

        // 給前台用的有效公告
        // GET: /api/announcement/active
        [HttpGet("active")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult<IEnumerable<CAnnouncementDTO>>> GetActive(CancellationToken ct)
        {
            var announcement = await _announcementService.GetActiveAsync(ct);

            return Ok(announcement);
        }

        // GET: /api/announcement/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CAnnouncementDTO>> Get(int id, CancellationToken ct)
        {
            var announcement = await _announcementService.GetAsync(id, ct);
            return announcement is null ? NotFound() : Ok(announcement);
        }

        //POST: /api/announcements
       [HttpPost]
        public async Task<ActionResult<CAnnouncementDTO>> Create([FromBody] CSaveAnnouncementDTO dto, CancellationToken ct)
        {
            var created = await _announcementService.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
    }
}
