using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth;

namespace ApiProject.Services
{
    public class CMemberServices:IMemberService
    {
        private readonly dbFurniMartContext _context;
        private readonly IPasswordHasher<TMember> _hasher;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public CMemberServices(dbFurniMartContext context, IPasswordHasher<TMember> hasher, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env, IConfiguration config)
        {
            _context = context;
            _hasher = hasher;
            _http = httpContextAccessor;
            _env = env;
            _config = config;
        }

        //註冊帳號
        public async Task<ResultDTO> MemberCreateAccountAsync(ReqMemberCreateAccountDTO reqdto,CancellationToken ct = default)
        {
            // 1) 基礎驗證
            if (!Regex.IsMatch(reqdto.Phone ?? "", @"^\d{10}$"))
                throw new InvalidOperationException("手機號碼格式不正確，需為10位數字。");

            if (string.IsNullOrWhiteSpace(reqdto.Password) || reqdto.Password.Length < 6)
                throw new InvalidOperationException("密碼長度至少 6 碼。");

            if (string.IsNullOrWhiteSpace(reqdto.Email) ||
                !Regex.IsMatch(reqdto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
                throw new InvalidOperationException("Email 格式不正確。");

            // 2) 唯一性檢查（帳號 / 手機 / Email）
            if (await _context.TMembers.AsNoTracking()
                .AnyAsync(x => x.FAccount == reqdto.Account, ct))
                throw new InvalidOperationException("此帳號已存在，請重新輸入");

            if (await _context.TMembers.AsNoTracking()
                .AnyAsync(x => x.FPhone == reqdto.Phone, ct))
                throw new InvalidOperationException("此手機號碼已存在，請重新輸入");

            if (await _context.TMembers.AsNoTracking()
                .AnyAsync(x => x.FEmail == reqdto.Email, ct))
                throw new InvalidOperationException("此 Email 已存在，請重新輸入");

            // 3) 信箱驗證檢查（新增的重點）
            //    規則：這個 Email 必須有一筆 tEmailVerification 紀錄，
            //    而且那筆：
            //      - FIsUsed = true  (表示使用者在 /verify-email-code 成功過)
            //      - FExpireTime 尚未過期 (或你想放寬，允許過期也行，我這裡還是檢查時效)
            //
            //    如果找不到，代表使用者沒有做過「email 驗證碼驗證」→ 不給註冊
            //
            var normalizedEmail = (reqdto.Email ?? "").Trim().ToLower();

            var emailVerifiedRecord = await _context.TEmailVerifications
                .Where(v =>
                    v.FIsUsed == true &&
                    v.FEmail.ToLower() == normalizedEmail)
                .OrderByDescending(v => v.FCreateTime)
                .FirstOrDefaultAsync(ct);

            if (emailVerifiedRecord == null)
            {
                throw new InvalidOperationException("請先通過信箱驗證再註冊帳號。");
            }

            // 這裡跟 VerifyEmailCodeAsync 同一個時間基準
            if (DateTime.Now > emailVerifiedRecord.FExpireTime)
            {
                throw new InvalidOperationException("驗證碼已過期，請重新驗證信箱後再註冊。");
            }

            // 4) 建立會員 Entity
            var entity = new TMember
            {
                FName = reqdto.Name,
                FPhone = reqdto.Phone,
                FEmail = reqdto.Email,
                FAccount = reqdto.Account,

                // 預設值
                FMemberImage = "default.png",
                FPhoneState = false,

                // ✅ 信箱已經驗證過了，所以這裡我們直接標記 true
                FEmailState = true,

                FLeveId = 1,
                FMoneySum = 0,
                FStatus = 1,
                FCreatTime = DateTime.Now,
                FUpdateTime = DateTime.Now
            };

            // 5) 密碼加鹽 (你原本做的正確作法，保留)
            entity.FPasswords = _hasher.HashPassword(entity, reqdto.Password);

            // 6) 存到資料庫
            _context.TMembers.Add(entity);
            await _context.SaveChangesAsync(ct);

            // 7) 回傳統一格式
            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "註冊帳號成功"
            };
        }

        //登入
        public async Task<ResMemberDTO?> MemberLoginAsync(ReqMemberLoginDTO reqdto, CancellationToken ct = default)
        {
            // 1) 查帳號
            var member = await _context.TMembers
                .AsNoTracking()
                .Include(m => m.FGenderNavigation)
                .Include(m => m.FStatusNavigation)
                .Include(m => m.FLeveIdNavigation)
                .FirstOrDefaultAsync(m => m.FAccount == reqdto.Account, ct);
            if (member == null)
                return null; // 或丟 InvalidOperationException 也可

            // 2) 驗證密碼
            var vr = _hasher.VerifyHashedPassword(member, member.FPasswords, reqdto.Password);
            if (vr == PasswordVerificationResult.Failed)
                return null;

            // 3) 若需 rehash（可選）
            if (vr == PasswordVerificationResult.SuccessRehashNeeded)
            {
                var tracked = await _context.TMembers.FirstAsync(m => m.FMemberId == member.FMemberId, ct);
                tracked.FPasswords = _hasher.HashPassword(tracked, reqdto.Password);
                await _context.SaveChangesAsync(ct);
            }

            // 4) 回傳會員資訊（Controller 會用來寫 Cookie）
            return new ResMemberDTO
            {
                MemberId = member.FMemberId,
                Account = member.FAccount,
                DisplayName = member.FDisplayName,
                Name = member.FName,
                Gender = member.FGender,
                GenderName = member.FGenderNavigation?.FGenderName,   // ← 依你的欄位名
                BirthDate = member.FBirthDate,
                Phone = member.FPhone,
                Email = member.FEmail,       // 若你已新增欄位
                Address = member.FAddress,
                MemberImage = member.FMemberImage,
                LevelId = member.FLeveId,
                LevelName = member.FLeveIdNavigation?.FLevelName,
                MoneySum = member.FMoneySum,
                Status = member.FStatus,
                StatusName = member.FStatusNavigation?.FStatusName,   // ← 依你的欄位名
                CreateTime = member.FCreatTime,
                UpdateTime = member.FUpdateTime
            };
        }

        //填寫會員資料及可改手機和Email
        public async Task MemberUpdateMeAsync(int memberId, ReqMemberUpdateDTO req, CancellationToken ct = default)
        {
            var member = await _context.TMembers.FirstOrDefaultAsync(m => m.FMemberId == memberId, ct);
            if (member == null)
                throw new InvalidOperationException("找不到會員資料");

            // ✅ 檢查手機是否重複（排除自己）
            if (!string.IsNullOrWhiteSpace(req.Phone))
            {
                bool phoneExists = await _context.TMembers
                    .AsNoTracking()
                    .AnyAsync(m => m.FPhone == req.Phone && m.FMemberId != memberId, ct);
                if (phoneExists)
                    throw new InvalidOperationException("此手機號碼已被其他會員使用");
            }

            // ✅ 檢查 Email 是否重複（排除自己）
            if (!string.IsNullOrWhiteSpace(req.Email))
            {
                bool emailExists = await _context.TMembers
                    .AsNoTracking()
                    .AnyAsync(m => m.FEmail == req.Email && m.FMemberId != memberId, ct);
                if (emailExists)
                    throw new InvalidOperationException("此 Email 已被其他會員使用");
            }

            // ✅ 更新可修改欄位
            member.FDisplayName = req.DisplayName;
            member.FName = req.Name;
            member.FGender = req.Gender;
            member.FPhone = req.Phone;
            member.FEmail = req.Email;
            member.FAddress = req.Address;
            member.FUpdateTime = DateTime.Now;

            await _context.SaveChangesAsync(ct);
        }

        //取得會員資料
        public async Task<ResMemberDTO> GetMemberMeAsync(int memberId, CancellationToken ct = default)
        {
            // 1️⃣ 先查會員
            var member = await _context.TMembers
                .AsNoTracking()
                .Include(m => m.FGenderNavigation)
                .Include(m => m.FStatusNavigation)
                .Include(m => m.FLeveIdNavigation)
                .FirstOrDefaultAsync(m => m.FMemberId == memberId, ct);

            // 2️⃣ 沒找到就拋出錯誤
            if (member == null)
                throw new InvalidOperationException("找不到會員資料，請重新登入。");

            // 3️⃣ 組成要回傳的 DTO
            var dto = new ResMemberDTO
            {
                MemberId = member.FMemberId,
                Account = member.FAccount,
                DisplayName = member.FDisplayName,
                Name = member.FName,
                Gender = member.FGender,
                GenderName = member.FGenderNavigation?.FGenderName,   // ← 依你的欄位名
                BirthDate = member.FBirthDate,
                Phone = member.FPhone,
                Email = member.FEmail,       // 若你已新增欄位
                Address = member.FAddress,
                MemberImage = member.FMemberImage,
                LevelId = member.FLeveId,
                LevelName = member.FLeveIdNavigation?.FLevelName,
                MoneySum = member.FMoneySum,
                Status = member.FStatus,
                StatusName = member.FStatusNavigation?.FStatusName,   // ← 依你的欄位名
                CreateTime = member.FCreatTime,
                UpdateTime = member.FUpdateTime
            };

            //// 4️⃣ 若要補上完整圖片 URL，可以這樣組
            //if (!string.IsNullOrEmpty(dto.MemberImage))
            //{
            //    // 假設你有設定檔案根路徑，例如 http://localhost:7131/MemberHeadImages/
            //    string baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
            //    dto.MemberImage = $"{baseUrl}/MemberHeadImages/{dto.MemberImage}";
            //}

            return dto;
        }

        //登出
        public async Task<ResultDTO> MemberLogoutAsync(CancellationToken ct = default)
        {
            // 取當前 HttpContext
            var httpCtx = _http.HttpContext ?? throw new InvalidOperationException("無法取得目前的 HTTP 內容");

            // ✅ 清除 Session
            httpCtx.Session.Clear();

            // ✅ 登出 Cookie（會讓 [Authorize] 失效）
            //await httpCtx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "已登出"
            };
        }

        //修改密碼
        public async Task<ResultDTO> MemberUpdatePasswordAsync(int memberId, ReqMemberUpdatePasswordDTO req, CancellationToken ct = default)
        {
            // 1) 基本驗證
            if (string.IsNullOrWhiteSpace(req.OldPassword) || string.IsNullOrWhiteSpace(req.NewPassword))
                throw new InvalidOperationException("密碼欄位不可空白");

            if (!string.IsNullOrWhiteSpace(req.ConfirmNewPassword) &&
                !string.Equals(req.NewPassword, req.ConfirmNewPassword))
                throw new InvalidOperationException("兩次輸入的新密碼不一致");

            if (req.NewPassword.Length < 6)
                throw new InvalidOperationException("新密碼長度至少需 6 碼");

            // 2) 取得會員（需追蹤）
            var member = await _context.TMembers.FirstOrDefaultAsync(m => m.FMemberId == memberId, ct);
            if (member == null)
                throw new InvalidOperationException("找不到會員資料");

            // 3) 驗證舊密碼
            var verify = _hasher.VerifyHashedPassword(member, member.FPasswords, req.OldPassword);
            if (verify == PasswordVerificationResult.Failed)
                throw new InvalidOperationException("舊密碼不正確");

            // 4) 新密碼不可與舊密碼相同
            var sameAsOld = _hasher.VerifyHashedPassword(member, member.FPasswords, req.NewPassword);
            if (sameAsOld == PasswordVerificationResult.Success)
                throw new InvalidOperationException("新密碼不可與舊密碼相同");

            // 5) 產生新雜湊並更新
            member.FPasswords = _hasher.HashPassword(member, req.NewPassword);
            member.FUpdateTime = DateTime.Now;

            await _context.SaveChangesAsync(ct);

            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "密碼已更新"
            };
        }

        //確認目前密碼
        public async Task<bool> CheckPasswordAsync(int memberId, string rawPassword, CancellationToken ct = default)
        {
            // 撈目前會員
            var member = await _context.TMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.FMemberId == memberId, ct);

            if (member == null) return false;
            if (string.IsNullOrEmpty(rawPassword)) return false;

            // 這邊假設資料庫裡的加密密碼欄位叫 FPassword
            // 也假設你在登入時用的是 PasswordHasher<TMember>
            var result = _hasher.VerifyHashedPassword(member, member.FPasswords, rawPassword);

            return result == PasswordVerificationResult.Success;
        }

        //產生 6 碼驗證碼
        private string GenerateCode()
        {
            var rand = new Random();
            return rand.Next(100000, 999999).ToString(); // 6碼驗證碼
        }

        private async Task SendEmailAsync(string toEmail, string code)
        {
            // 1. 從 appsettings.json 讀 EmailSettings 區塊
            var emailSection = _config.GetSection("EmailSettings");
            var host = emailSection["Host"];            // smtp.gmail.com
            var portRaw = emailSection["Port"];         // "587"
            var enableSslRaw = emailSection["EnableSSL"]; // "true"
            var userName = emailSection["UserName"];    // 你的gmail@gmail.com
            var password = emailSection["Password"];    // 應用程式密碼(16碼)

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portRaw) ||
                string.IsNullOrWhiteSpace(enableSslRaw) ||
                string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(password))
            {
                // 如果你要在除錯時更清楚，也可以丟例外
                throw new InvalidOperationException("寄信設定不完整，請確認 appsettings.json 的 EmailSettings。");
            }

            int port = int.Parse(portRaw);
            bool enableSsl = bool.Parse(enableSslRaw);

            // 2. 準備信件內容
            //    這封就是使用者收到的驗證碼信
            var mail = new MailMessage();
            mail.From = new MailAddress(userName, "FurniMart 驗證中心"); // 第二個參數是顯示名稱，可以改
            mail.To.Add(toEmail);
            mail.Subject = "您的驗證碼";
            mail.Body =
                $"您好！\r\n\r\n" +
                $"您的驗證碼是：{code}\r\n" +
                $"此驗證碼 5 分鐘內有效，請不要告訴別人。\r\n\r\n" +
                $"FurniMart 敬上";
            mail.IsBodyHtml = false; // 如果你想做漂亮一點的 HTML，可以改成 true 並組 HTML

            // 3. 建 SMTP Client，連到 Gmail
            using (var smtp = new SmtpClient(host, port))
            {
                smtp.EnableSsl = enableSsl; // Gmail 587 走 TLS
                smtp.Credentials = new NetworkCredential(userName, password);

                // 4. 寄信 (這是 async 版本)
                await smtp.SendMailAsync(mail);
            }
        }

        public async Task<ResultDTO> SendEmailVerificationCodeAsync(string email)
        {
            // 1. 產生驗證碼
            string code = GenerateCode();

            // 2. 建一筆 DB 記錄 (5 分鐘有效)
            var entity = new TEmailVerification
            {
                FEmail = (email ?? "").Trim(),
                FCode = code,
                FExpireTime = DateTime.Now.AddMinutes(5),
                FIsUsed = false,
                FCreateTime = DateTime.Now
            };

            _context.TEmailVerifications.Add(entity);
            await _context.SaveChangesAsync();

            // 3. 寄信
            await SendEmailAsync(email, code);

            // 4. 回傳
            return new ResultDTO
            {
                Ok = true,
                Code = 200,
                Message = "驗證碼已寄出，請至信箱查看。"
            };
        }
        //前端輸入驗證碼
        public async Task<ResultDTO> VerifyEmailCodeAsync(string email, string code)
        {
            email = (email ?? "").Trim().ToLower();
            code = (code ?? "").Trim();
            // 找最近的一筆該 email + code，還沒用掉的
            var record = await _context.TEmailVerifications
                .Where(v => v.FEmail == email && v.FCode == code && v.FIsUsed == false)
                .OrderByDescending(v => v.FCreateTime)
                .FirstOrDefaultAsync();

            if (record == null)
            {
                return new ResultDTO
                {
                    Ok = false,
                    Code = 400,
                    Message = "驗證碼錯誤。"
                };
            }

            // 檢查過期
            if (DateTime.Now > record.FExpireTime)
            {
                return new ResultDTO
                {
                    Ok = false,
                    Code = 401,
                    Message = "驗證碼已過期。"
                };
            }

            // 標記成已使用
            record.FIsUsed = true;
            await _context.SaveChangesAsync();

            return new ResultDTO
            {
                Ok = true,
                Code = 200,
                Message = "信箱驗證成功。"
            };
        }

        //寄重設密碼驗證碼，檢查「帳號+Email 是否存在同一個會員」
        public async Task<ResultDTO> SendResetPasswordCodeAsync(string account, string email)
        {
            // 正規化輸入
            var acc = (account ?? "").Trim();
            var mail = (email ?? "").Trim();

            if (string.IsNullOrWhiteSpace(acc) || string.IsNullOrWhiteSpace(mail))
            {
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "帳號與 Email 必填"
                };
            }

            // 1. 確認這個帳號+Email 是同一個會員
            var member = await _context.TMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.FAccount == acc && m.FEmail == mail);

            if (member == null)
            {
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "帳號或 Email 不正確"
                };
            }

            // 2. 產生驗證碼
            var code = GenerateCode();

            // 3. 寫入驗證碼紀錄 (跟註冊驗證碼一樣邏輯, 5 分鐘有效)
            var entity = new TEmailVerification
            {
                FEmail = mail,
                FCode = code,
                FExpireTime = DateTime.Now.AddMinutes(5),
                FIsUsed = false,
                FCreateTime = DateTime.Now
                // 如果你未來想區分用途，可以在資料表加 FType="reset"
            };

            _context.TEmailVerifications.Add(entity);
            await _context.SaveChangesAsync();

            // 4. 寄信 (重用 SendEmailAsync)
            await SendEmailAsync(mail, code);

            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "重設密碼驗證碼已寄出，請至信箱查看。"
            };
        }

        //驗證碼 + 重設密碼
        public async Task<ResultDTO> ResetPasswordAsync(
            string account,
            string email,
            string code,
            string newPassword,
            string? confirmNewPassword,
            CancellationToken ct = default)
        {
            var acc = (account ?? "").Trim();
            var mail = (email ?? "").Trim().ToLower();
            var c = (code ?? "").Trim();

            // 0. 基本檢查
            if (string.IsNullOrWhiteSpace(acc) ||
                string.IsNullOrWhiteSpace(mail) ||
                string.IsNullOrWhiteSpace(c) ||
                string.IsNullOrWhiteSpace(newPassword))
            {
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "資料不完整"
                };
            }

            if (!string.IsNullOrWhiteSpace(confirmNewPassword) &&
                !string.Equals(newPassword, confirmNewPassword))
            {
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "兩次密碼不一致"
                };
            }

            if (newPassword.Length < 6)
            {
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "新密碼至少需 6 碼"
                };
            }

            // 1. 找會員 (要可追蹤，因為等一下要改密碼)
            var member = await _context.TMembers
                .FirstOrDefaultAsync(m => m.FAccount == acc && m.FEmail.ToLower() == mail, ct);

            if (member == null)
            {
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "帳號或 Email 不正確"
                };
            }

            // 2. 確認驗證碼 (找最新一筆, 未使用, 沒過期)
            var record = await _context.TEmailVerifications
                .Where(v =>
                    v.FEmail.ToLower() == mail &&
                    v.FCode == c &&
                    v.FIsUsed == false)
                .OrderByDescending(v => v.FCreateTime)
                .FirstOrDefaultAsync(ct);

            if (record == null)
            {
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "驗證碼錯誤"
                };
            }

            if (DateTime.Now > record.FExpireTime)
            {
                return new ResultDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "驗證碼已過期"
                };
            }

            // 3. 標記驗證碼已用
            record.FIsUsed = true;

            // 4. 更新會員密碼（雜湊後存進 FPasswords）
            member.FPasswords = _hasher.HashPassword(member, newPassword);
            member.FUpdateTime = DateTime.Now;

            await _context.SaveChangesAsync(ct);

            return new ResultDTO
            {
                Ok = true,
                Code = StatusCodes.Status200OK,
                Message = "密碼已更新，請使用新密碼登入"
            };
        }

        // === Google OAuth 登入主流程 ===
        public async Task<ResMemberDTO> GoogleOauthSignInAsync(string idToken, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(idToken))
                throw new InvalidOperationException("缺少 idToken");

            // 1) 驗證 Google ID Token 是否合法，audience 必須是你的 ClientId
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["Google:ClientId"] }
                });

            var googleSub = payload.Subject;                 // Google 唯一使用者 ID
            var email = payload.Email;
            var name = payload.Name ?? payload.GivenName ?? email;
            var picture = payload.Picture;                   // Google 大頭貼（可參考）

            // 2) 查有沒有既有的外部綁定 (做法A：獨立表)
            var bind = await _context.TExternalLogins
                .Include(x => x.FMember)
                .FirstOrDefaultAsync(x => x.FProvider == "google" && x.FProviderUserId == googleSub, ct);

            TMember member;

            if (bind != null)
            {
                member = bind.FMember;
            }
            else
            {
                // 3) 沒綁定：用 Email 找本地帳號，避免重複
                member = await _context.TMembers.FirstOrDefaultAsync(m => m.FEmail == email, ct);

                if (member == null)
                {
                    // 3-1) 沒有就建立一個本地會員（給一些預設值）
                    member = new TMember
                    {
                        FName = name,
                        FEmail = email,
                        FAccount = "g_" + Guid.NewGuid().ToString("N")[..10],
                        FMemberImage = "default.png",
                        FPhoneState = false,
                        FEmailState = true,  // Google 已驗過信
                        FLeveId = 1,
                        FMoneySum = 0,
                        FStatus = 1,
                        FCreatTime = DateTime.Now,
                        FUpdateTime = DateTime.Now
                    };
                    _context.TMembers.Add(member);
                    await _context.SaveChangesAsync(ct);
                }

                // 3-2) 新增外部綁定紀錄 (做法A)
                var ext = new TExternalLogin
                {
                    FMemberId = member.FMemberId,
                    FProvider = "google",
                    FProviderUserId = googleSub,
                    FEmail = email,
                    FDisplayName = name,
                    FAvatarUrl = picture,
                    FCreateTime = DateTime.Now,
                    FUpdateTime = DateTime.Now
                };
                _context.TExternalLogins.Add(ext);
                await _context.SaveChangesAsync(ct);
            }

            // 4) 回傳給前端用的會員 DTO（沿用你系統現有格式）
            return ToResMemberDTO(member);
        }

        // === 將 TMember 轉成 ResMemberDTO（沿用你 GetMemberMeAsync 的映射邏輯）===
        private ResMemberDTO ToResMemberDTO(TMember member)
        {
            // 如果你想包含導航屬性名稱，可視需要 Include 再取；這裡用安全 Null-conditional。
            return new ResMemberDTO
            {
                MemberId = member.FMemberId,
                Account = member.FAccount,
                DisplayName = member.FDisplayName,
                Name = member.FName,
                Gender = member.FGender,
                GenderName = member.FGenderNavigation?.FGenderName,
                BirthDate = member.FBirthDate,
                Phone = member.FPhone,
                Email = member.FEmail,
                Address = member.FAddress,
                MemberImage = member.FMemberImage, // 前端會用你的 toImageUrl() 組完整 URL
                LevelId = member.FLeveId,
                LevelName = member.FLeveIdNavigation?.FLevelName,
                MoneySum = member.FMoneySum,
                Status = member.FStatus,
                StatusName = member.FStatusNavigation?.FStatusName,
                CreateTime = member.FCreatTime,
                UpdateTime = member.FUpdateTime
            };
        }

        //註冊驗證是否重複
        public async Task<bool> IsAccountTakenAsync(string account)
        {
            if (string.IsNullOrWhiteSpace(account)) return false;
            return await _context.TMembers.AnyAsync(m => m.FAccount == account);
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return await _context.TMembers.AnyAsync(m => m.FEmail == email);
        }

        public async Task<bool> IsPhoneTakenAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return await _context.TMembers.AnyAsync(m => m.FPhone == phone);
        }

        public async Task<ResUniqueCheckDTO> CheckUniqueAsync(ReqUniqueCheckDTO req)
        {
            var res = new ResUniqueCheckDTO
            {
                AccountTaken = await IsAccountTakenAsync(req.Account ?? ""),
                EmailTaken = await IsEmailTakenAsync(req.Email ?? ""),
                PhoneTaken = await IsPhoneTakenAsync(req.Phone ?? "")
            };
            return res;
        }
    }
}
