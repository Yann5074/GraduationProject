using ApiProject.DTOs;
using ApiProject.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;
        public MemberController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        // 1) 註冊
        // POST /api/members/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ReqMemberCreateDTO req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var result = await _memberService.MemberCreateAsync(req, ct);

                // Service 會回一個 ResultDTO（內含 Ok、Code…），
                // 這裡用它的 Code 當作 HTTP 狀態碼，一併把物件回給前端
                return StatusCode(result.Code, result);

                // 若未來改成回傳新會員ID（例如 newMemberId），比較語意化的作法：
                // return CreatedAtAction(nameof(Me), new { }, new { memberId = newId });
            }
            catch (InvalidOperationException ex)
            {
                // Service 主動丟出的業務錯誤（像「手機/帳號已存在」、「格式不正確」）
                // 轉成 400 Bad Request，訊息給前端顯示
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2) 登入
        // POST /api/members/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ReqMemberLoginDTO req, CancellationToken ct)
        {
            try
            {
                var result = await _memberService.MemberLoginAsync(req, ct);
                return StatusCode(result.Code, result);
            }
            catch (InvalidOperationException ex)
            {
                // 回傳 400 給前端
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
