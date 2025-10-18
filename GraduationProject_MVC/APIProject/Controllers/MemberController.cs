using ApiProject.Dictionary;
using ApiProject.DTOs;
using ApiProject.Infrastructure;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly dbFurniMartContext _db;
        private readonly IWebHostEnvironment _env;
        public MemberController(IMemberService memberService, dbFurniMartContext db, IWebHostEnvironment env)
        {
            _memberService = memberService;
            _db = db;
            _env = env;
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

            ////建立「Claims」（身分資訊）
            //var claims = new List<Claim>
            //    {
            //        new Claim(ClaimTypes.NameIdentifier, m.MemberId.ToString()),
            //        new Claim(ClaimTypes.Name, m.Account),
            //        new Claim("displayName", m.DisplayName ?? string.Empty),
            //    };
            ////建立「身份（Identity）」與「主體（Principal）」
            //var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //var principal = new ClaimsPrincipal(identity);
            ////寫入 Cookie（登入成功）

            //await HttpContext.SignInAsync(
            //    CookieAuthenticationDefaults.AuthenticationScheme,
            //    principal,
            //    new AuthenticationProperties { IsPersistent = true });

            // (B) ✅ CDictionary + Session：把常用資訊放進去
            HttpContext.Session.Clear(); // ✅ 防 session 固定攻擊
            HttpContext.Session.SetString(CMemberDictionary.SK_LOGIN_ID, m.MemberId.ToString());
            HttpContext.Session.SetString(CMemberDictionary.SK_LOGIN_ACCOUNT, m.Account);
            HttpContext.Session.SetString(CMemberDictionary.SK_LOGIN_NAME, m.DisplayName ?? string.Empty);

            // 整包 DTO（選用）
            HttpContext.Session.SetObject(CMemberDictionary.SK_LOGIN_OBJECT, m);
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

        // 5) 取得個資
        // GET /api/members/me
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var dto = HttpContext.Session.GetObject<ResMemberDTO>(CMemberDictionary.SK_LOGIN_OBJECT);
            if (dto != null)
                return Ok(dto);

            // 萬一只存了簡單欄位，備用
            var idStr = HttpContext.Session.GetString(CMemberDictionary.SK_LOGIN_ID);
            var acct = HttpContext.Session.GetString(CMemberDictionary.SK_LOGIN_ACCOUNT);
            var display = HttpContext.Session.GetString(CMemberDictionary.SK_LOGIN_NAME);

            if (string.IsNullOrEmpty(idStr))
                return Unauthorized(new { message = "Session 過期，請重新登入" });

            return Ok(new
            {
                MemberId = int.Parse(idStr),
                Account = acct,
                DisplayName = display
            });

        }

        // 6) 修改密碼
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

        // 6) 上傳大頭貼
        //// POST /api/members/UploadPhoto
        //[Authorize]
        //[HttpPost("me/UploadPhoto")]
        //public async Task<ActionResult<ResMemberUploadPhotoDTO>> UploadPhoto([FromForm] UploadPhotoForm form, CancellationToken ct)
        //{
        //    try
        //    {
        //        //從登入者的 Cookie（Claims）取得會員 ID
        //        var memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        //        var res = await _memberService.MemberUploadPhotoAsync(memberId, file, ct);
        //        return Ok(res); // { url, fileName }
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        // 7) 上傳大頭貼
        // POST /api/member/me/uploadphoto
        [Authorize]
        [HttpPost("me/uploadphoto")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 2_000_000)] // 2MB
        public async Task<ActionResult<object>> UploadPhoto([FromForm] UploadPhotoForm form, CancellationToken ct)
        {
            var file = form.File;
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "未選擇檔案" });

            // 1) 基本限制
            const long MAX_BYTES = 2 * 1024 * 1024; // 2MB
            if (file.Length > MAX_BYTES) return BadRequest(new { message = "檔案過大，限制 2MB 以內" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var okExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!okExts.Contains(ext)) return BadRequest(new { message = "僅支援 jpg、jpeg、png、webp" });

            var okContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!okContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return BadRequest(new { message = "Content-Type 不正確" });

            // 2) 取得登入者 Id（只用 Session 也可：int.Parse(HttpContext.Session.GetString(...))）
            var memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // 3) 取會員（要追蹤）
            var member = await _db.TMembers.FirstOrDefaultAsync(m => m.FMemberId == memberId, ct);
            if (member == null) return BadRequest(new { message = "找不到會員資料" });

            // 4) 準備「共用資料夾」與檔名  🔁 這段改成 SharedStorage
            //    方案根/SharedStorage/MemberHeadImages
            var sharedRoot = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), "..", "SharedStorage", "MemberHeadImages")
            );
            Directory.CreateDirectory(sharedRoot);

            var fileName = $"{memberId}_{DateTime.UtcNow.Ticks}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(sharedRoot, fileName);

            // 5) 寫檔
            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream, ct);
            }

            // 6) 刪舊檔（若不是預設） 🔁 也要用 sharedRoot
            if (!string.IsNullOrWhiteSpace(member.FMemberImage) &&
                !string.Equals(member.FMemberImage, "default.png", StringComparison.OrdinalIgnoreCase))
            {
                var oldPath = Path.Combine(sharedRoot, member.FMemberImage);
                if (System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { /* ignore */ }
                }
            }

            // 7) 更新 DB
            member.FMemberImage = fileName;
            member.FUpdateTime = DateTime.Now;
            await _db.SaveChangesAsync(ct);

            // 8) 回傳路徑
            //    只要兩個專案 Program.cs 都把 /MemberHeadImages 映射到 sharedRoot，
            //    這裡可以回相對路徑，也可以回完整 URL（都能用）
            var relative = $"/MemberHeadImages/{fileName}";
            var absolute = $"{Request.Scheme}://{Request.Host}{relative}";

            return Ok(new { url = absolute, relative, fileName });
        }



    }
}
