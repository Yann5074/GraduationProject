using ApiProject.DTOs;
using ApiProject.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers
{
    [ApiController]
    [Route("api/rec")]
    public class RecommendController : Controller
    {
        private readonly IRecommonedService _recom;

        public RecommendController(IRecommonedService recom)
        {
            _recom = recom;
        }

        //Item -> Item 共訪推薦系統 (變體卡)
        // Get: api/rec/item/{productId:int}/variants
        [HttpGet("item/{productId:int}/variants")]
        public async Task<ActionResult<IReadOnlyList<ResRecommonedDTO>>> ItemVariants(int productId, [FromQuery] int take=12, [FromQuery] int k = 2, [FromQuery] int windowDay = 30, [FromQuery] int sessionWin = 30, CancellationToken ct = default)
        {
            var result = await _recom.ItemToItemRecommonedAsync(productId, take, k, windowDay, sessionWin, ct);
            return Ok(result);
        }

        //立刻刷新
        // Post: api/rec/rebuild
        [HttpPost("rebuild")]
        public async Task<IActionResult> Rebuild(CancellationToken ct)
        {
            var result = _recom.InvalidateAllAsync();
            var res = new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "緩存已失效，下次請求將重新計算"
            };
            return Ok(res);
        }

        // 熱門推薦
        // Get: api/rec/trending
        [HttpGet("trending")]
        public async Task<ActionResult<IReadOnlyList<ResRecommonedDTO>>> Trending([FromQuery] int take = 12, [FromQuery] int k = 2, [FromQuery] int windowDays = 30, CancellationToken ct = default)
        {
            var reuslt = await _recom.TrendingAsync(take, k, windowDays, ct);
            return Ok(reuslt);
        }

        //訪客推薦
        // Get: api/rec/session/{sessionId}
        [HttpGet("session/{sessionId}")]
        public async Task<ActionResult<IReadOnlyList<ResRecommonedDTO>>> ForSession(string sessionId, [FromQuery] int take = 12, [FromQuery] int k = 2, [FromQuery] int windowDays = 30, [FromQuery] int sessionWin = 30, CancellationToken ct = default)
        {
            var result = await _recom.ForSessionAsync(sessionId, take, k, windowDays, sessionWin, ct);
            return Ok(result);
        }
    }
}
