using ApiProject.DTOs;
using ApiProject.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _AnalyticsService;
        public AnalyticsController(IAnalyticsService AnalyticsService) => _AnalyticsService = AnalyticsService;

        /// 熱銷排行（不同商品）— 只回傳上架中商品
        [HttpGet("best-sellers")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBestSellers(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? top,
            [FromQuery] string? statuses,
            CancellationToken ct)
        {
            var q = new CBestSellerQueryDTO
            {
                From = from,
                To = to,
                Top = top ?? 5,
                Statuses = statuses
            };
            var items = await _AnalyticsService.GetBestSellersAsync(q, ct);
            return Ok(new { items });
        }

    }
}
