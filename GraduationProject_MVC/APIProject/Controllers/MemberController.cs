using ApiProject.DTOs;
using ApiProject.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public async Task<IActionResult> Create([FromBody] ReqMemberCreateAccountDTO req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var result = await _memberService.MemberCreateAccountAsync(req, ct);

                // 可選：註冊後直接登入（Cookie）
                //await SignInAsync(entityIdOrAccount);

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

        // 2) 填寫會員資料及可改手機和Email
        // PUT /api/members/UpdateMe
        [Authorize]
        [HttpPut("UpdateMe")]
        public async Task<IActionResult> UpdateMe([FromBody] ReqMemberUpdateDTO req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _memberService.MemberUpdateMeAsync(memberId, req, ct);
                return NoContent(); // 204：更新成功無內容
            }
            catch (InvalidOperationException ex)
            {
                // 例如：手機/Email 重複、資料不存在…（你在 Service 丟的）
                return BadRequest(new { message = ex.Message });
            }
        }

        // 3) 登入
        // POST /api/members/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ReqMemberLoginDTO req, CancellationToken ct)
        {
            //呼叫 Service 驗證帳密
            var m = await _memberService.MemberLoginAsync(req, ct);
            if (m == null) return Unauthorized(new { message = "帳號或密碼錯誤" });

            //建立「Claims」（身分資訊）
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, m.MemberId.ToString()),
                    new Claim(ClaimTypes.Name, m.Account),
                    new Claim("displayName", m.DisplayName ?? string.Empty),
                };
            //建立「身份（Identity）」與「主體（Principal）」
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            //寫入 Cookie（登入成功）
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });

            return Ok(m); // 或 Ok(m) 也可
        }

        // 4) 登出
        // POST /api/members/logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var result = await _memberService.MemberLogoutAsync(ct);
            // 若你想遵守 204 無內容，也可直接 return NoContent();
            return StatusCode(result.Code, result); // 這裡選擇 200 + 訊息，前端好顯示
        }

        // 5) 修改密碼
        // PUT /api/members/me/UpdatePassword
        [Authorize]
        [HttpPut("me/UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] ReqMemberUpdatePasswordDTO req, CancellationToken ct)
        {
            //ASP.NET Core 在接收 [FromBody] ReqMemberChangePasswordDTO req 時，會自動幫你檢查欄位是否符合資料模型（例如必填欄位、格式）。
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            //從登入者的 Cookie（Claims）取得會員 ID
            var memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var result = await _memberService.MemberUpdatePasswordAsync(memberId, req, ct);
                return StatusCode(result.Code, result);   // 或 return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
