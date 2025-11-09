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

        // 熱銷排行（不同商品）— 只回傳上架中商品
        // GET: /api/Analytics/best-sellers 
        [HttpGet("best-sellers")]
        public async Task<IActionResult> GetBestSellers(int? year, int? top, string? statuses, CancellationToken ct)
        {
            int yearNow = year ?? DateTime.Now.Year;

            DateTime start = new DateTime(yearNow, 1, 1, 0, 0, 0);
            DateTime end = start.AddYears(1);

            var q = new CBestSellerQueryDTO
            {
                From = start,
                To = end,
                Top = top ?? 5,
                Statuses = statuses
            };
            var items = await _AnalyticsService.GetBestSellersAsync(q, ct);
            return Ok(new { items });
        }

    }
}
