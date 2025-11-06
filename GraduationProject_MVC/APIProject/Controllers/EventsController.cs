using ApiProject.DTOs;
using ApiProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : Controller
    {
        private readonly dbFurniMartContext _context;
        public EventsController(dbFurniMartContext context)
        {
            _context = context;
        }

        public sealed class ReqTrackEvent
        {
            public string? SessionId { get; set; }
            public int? ProductId { get; set; }
            public int? ProductVariantId { get; set; }
            public int? EventType { get; set; }
            public int? DwellSec {  get; set; }
            public int? UserId { get; set; }
        }

        //追蹤事件
        //Post: api/events/track
        [HttpPost("track")]
        public async Task<ResultDTO> Track([FromBody] ReqTrackEvent req, CancellationToken ct)
        {
            var e = new TUserEvent
            {
                FSessionId = req.SessionId,
                FProductId = req.ProductId,
                FProductVariantId = req.ProductVariantId,
                FEventType = (int)req.EventType,
                FDwellSec = req.DwellSec,
                FOccurredAt = DateTime.UtcNow,
                FUserId = req.UserId
            };
            _context.Add(e);
            await _context.SaveChangesAsync(ct);
            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "追蹤成功"
            };
        }
    }
}
