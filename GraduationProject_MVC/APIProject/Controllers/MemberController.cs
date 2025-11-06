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
        // POST /api/Member/create
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
        // PUT /api/Member/UpdateMe
        [Authorize]
        [HttpPut("UpdateMe")]
        public async Task<IActionResult> UpdateMe([FromBody] ReqMemberUpdateDTO req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _memberService.MemberUpdateMeAsync(memberId, req, ct);

                var refreshed = await _memberService.GetMemberMeAsync(memberId, ct);
                if (refreshed != null)
                    HttpContext.Session.SetObject(CMemberDictionary.SK_LOGIN_OBJECT, refreshed);

                return NoContent(); // 204：更新成功無內容
            }
            catch (InvalidOperationException ex)
            {
                // 例如：手機/Email 重複、資料不存在…（你在 Service 丟的）
                return BadRequest(new { message = ex.Message });
            }
        }

        // 3) 登入
        // POST /api/Member/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ReqMemberLoginDTO req, CancellationToken ct)
        {
            //呼叫 Service 驗證帳密
            var m = await _memberService.MemberLoginAsync(req, ct);
            if (m == null) return Unauthorized(new { message = "帳號或密碼錯誤" });
           
            // ✅ CDictionary + Session：把常用資訊放進去
            HttpContext.Session.Clear(); // ✅ 防 session 固定攻擊
            HttpContext.Session.SetString(CMemberDictionary.SK_LOGIN_ID, m.MemberId.ToString());
            HttpContext.Session.SetString(CMemberDictionary.SK_LOGIN_ACCOUNT, m.Account);
            HttpContext.Session.SetString(CMemberDictionary.SK_LOGIN_NAME, m.DisplayName ?? string.Empty);

            // 整包 DTO（選用）
            HttpContext.Session.SetObject(CMemberDictionary.SK_LOGIN_OBJECT, m);
            return Ok(m); // 或 Ok(m) 也可
        }

        // 4) 登出
        // POST /api/Member/logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var result = await _memberService.MemberLogoutAsync(ct);
            // 若你想遵守 204 無內容，也可直接 return NoContent();
            return StatusCode(result.Code, result); // 這裡選擇 200 + 訊息，前端好顯示
        }

        // 5) 取得個資
        // GET /api/Member/me
        [Authorize] // ✅ 只用 Session 時，SessionAuthHandler 會把 Session 轉成 Claims
        [HttpGet("me")]
        public async Task<ActionResult<ResMemberDTO>> GetMe(CancellationToken ct)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr))
                return Unauthorized(new { message = "Session 過期或未登入" });

            var cached = HttpContext.Session.GetObject<ResMemberDTO>(CMemberDictionary.SK_LOGIN_OBJECT);
            if (cached != null) return Ok(cached);

            try
            {
                var memberId = int.Parse(idStr);
                var dto = await _memberService.GetMemberMeAsync(memberId, ct);
                if (dto == null) return NotFound(new { message = "找不到會員資料" });

                HttpContext.Session.SetObject(CMemberDictionary.SK_LOGIN_OBJECT, dto);
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                // 將 service 的邏輯錯誤轉成 400/404，而不是讓它拋出到 pipeline
                return BadRequest(new { message = ex.Message });
            }
        }

        // 6) 修改密碼
        // PUT /api/Member/me/UpdatePassword
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
       
        // 7) 上傳大頭貼
        // POST /api/Member/me/uploadphoto
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

            // ✅ 緊接著加這兩行
            var refreshed = await _memberService.GetMemberMeAsync(memberId, ct);
            if (refreshed != null)
                HttpContext.Session.SetObject(CMemberDictionary.SK_LOGIN_OBJECT, refreshed);

            // 8) 回傳路徑
            //    只要兩個專案 Program.cs 都把 /MemberHeadImages 映射到 sharedRoot，
            //    這裡可以回相對路徑，也可以回完整 URL（都能用）
            var relative = $"/MemberHeadImages/{fileName}";
            var absolute = $"{Request.Scheme}://{Request.Host}{relative}";

            return Ok(new { url = absolute, relative, fileName });
        }

        // 8) 確認目前密碼
        [Authorize] // 只有登入的人能檢查
        // POST/api/Member/me/checkPassword
        [HttpPost("me/checkPassword")]
        public async Task<ActionResult<CheckPasswordResponseDTO>> CheckPassword(
            [FromBody] CheckPasswordRequestDTO req,
            CancellationToken ct)
        {
            // 從 Claims 取出當前登入會員ID
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr))
            {
                return Unauthorized(new CheckPasswordResponseDTO
                {
                    Ok = false,
                    Message = "尚未登入或 Session 已過期"
                });
            }

            if (!int.TryParse(idStr, out var memberId))
            {
                return BadRequest(new CheckPasswordResponseDTO
                {
                    Ok = false,
                    Message = "無效的會員識別"
                });
            }

            // 呼叫 service 驗證密碼
            bool ok = await _memberService.CheckPasswordAsync(memberId, req.Password ?? "", ct);

            return Ok(new CheckPasswordResponseDTO
            {
                Ok = ok,
                Message = ok ? "密碼正確" : "密碼錯誤"
            });
        }

        // 9) 產生 6 碼驗證碼
        // POST /api/Member/send-email-code
        [HttpPost("send-email-code")]
        public async Task<ActionResult<ResultDTO>> SendEmailCode([FromBody] ReqSendEmailCodeDTO req)
        {
            if (string.IsNullOrWhiteSpace(req.Email))
            {
                return BadRequest(new ResultDTO
                {
                    Ok = false,
                    Code = 400,
                    Message = "Email 必填"
                });
            }

            var result = await _memberService.SendEmailVerificationCodeAsync(req.Email);

            if (!result.Ok)
            {
                // 依你自己的 Code 決定要回哪個 HTTP 狀態碼
                return BadRequest(result);
            }

            return Ok(result);
        }

        // 10) 前端輸入驗證碼
        // POST /api/Member/verify-email-code
        [HttpPost("verify-email-code")]
        public async Task<ActionResult<ResultDTO>> VerifyEmailCode([FromBody] ReqVerifyEmailCodeDTO req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Code))
            {
                return BadRequest(new ResultDTO
                {
                    Ok = false,
                    Code = 400,
                    Message = "Email 與 驗證碼 必填"
                });
            }

            var result = await _memberService.VerifyEmailCodeAsync(req.Email, req.Code);

            if (!result.Ok)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // 11) 忘記密碼：寄送驗證碼
        // POST /api/Member/send-reset-code
        [AllowAnonymous]
        [HttpPost("send-reset-code")]
        public async Task<ActionResult<ResultDTO>> SendResetCode([FromBody] ReqSendResetCodeDTO req)
        {
            if (string.IsNullOrWhiteSpace(req.Account) || string.IsNullOrWhiteSpace(req.Email))
            {
                return BadRequest(new ResultDTO
                {
                    Ok = false,
                    Code = 400,
                    Message = "帳號與 Email 必填"
                });
            }

            var result = await _memberService.SendResetPasswordCodeAsync(req.Account, req.Email);

            if (!result.Ok)
            {
                // 帳號不存在 / Email 不吻合 之類
                return BadRequest(result);
            }

            return Ok(result);
        }

        // 12) 忘記密碼：驗證碼 + 重設密碼
        //POST /api/Member/reset-password
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<ActionResult<ResultDTO>> ResetPassword([FromBody] ReqResetPasswordDTO req, CancellationToken ct)
        {
            var result = await _memberService.ResetPasswordAsync(
                req.Account,
                req.Email,
                req.Code,
                req.NewPassword,
                req.ConfirmNewPassword,
                ct
            );

            if (!result.Ok)
            {
                // 例如 驗證碼錯誤 / 過期 / 密碼太短 / 兩次不一致
                // 我們用 BadRequest 回前端，前端可以直接顯示 result.Message
                return BadRequest(result);
            }

            return Ok(result);
        }

        // 13) Google 第三方登入
        // POST /api/Member/oauth/google
        [AllowAnonymous]
        [HttpPost("oauth/google")]
        public async Task<ActionResult<ResMemberDTO>> GoogleOauthSignIn([FromBody] ReqGoogleOauthDTO req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.IdToken))
                return BadRequest(new { ok = false, message = "缺少 Google IdToken" });

            try
            {
                // 1) 驗證 Google token + 取得/建立會員（你已在 Service 完成）
                var me = await _memberService.GoogleOauthSignInAsync(req.IdToken, ct);

                // 2) 取實體會員為了寫 Session（與帳密登入一致）
                var member = await _db.TMembers
                    .FirstOrDefaultAsync(m => m.FMemberId == me.MemberId, ct);

                if (member == null)
                    return NotFound(new { ok = false, message = "找不到會員資料" });

                // 3) 寫入 Session（沿用你 Login 的做法）
                HttpContext.Session.Clear(); // 防固定化攻擊
                HttpContext.Session.SetString(Dictionary.CMemberDictionary.SK_LOGIN_ID, member.FMemberId.ToString());
                HttpContext.Session.SetString(Dictionary.CMemberDictionary.SK_LOGIN_ACCOUNT, member.FAccount ?? string.Empty);
                HttpContext.Session.SetString(Dictionary.CMemberDictionary.SK_LOGIN_NAME, me.DisplayName ?? member.FName ?? string.Empty);
                HttpContext.Session.SetObject(Dictionary.CMemberDictionary.SK_LOGIN_OBJECT, me);

                // 4) 回傳會員 DTO 給前端
                return Ok(me);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, message = ex.Message });
            }
        }

        // 單一整合式檢查（推薦）
        [HttpPost("check-unique")]
        public async Task<IActionResult> CheckUnique([FromBody] ReqUniqueCheckDTO req)
        {
            var data = await _memberService.CheckUniqueAsync(req);
            return Ok(new ResultDTO { Ok = true, Code = 200, Data = data, Message = "OK" });
        }

        // 若想要逐一檢查，也可以提供這三支（可選）
        [HttpGet("check-account")]
        public async Task<IActionResult> CheckAccount([FromQuery] string account)
            => Ok(new ResultDTO { Ok = true, Code = 200, Data = new { taken = await _memberService.IsAccountTakenAsync(account) } });

        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
            => Ok(new ResultDTO { Ok = true, Code = 200, Data = new { taken = await _memberService.IsEmailTakenAsync(email) } });

        [HttpGet("check-phone")]
        public async Task<IActionResult> CheckPhone([FromQuery] string phone)
            => Ok(new ResultDTO { Ok = true, Code = 200, Data = new { taken = await _memberService.IsPhoneTakenAsync(phone) } });

    }
}
