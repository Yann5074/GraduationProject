using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ApiProject.Services
{
    public class CMemberServices:IMemberService
    {
        private readonly dbFurniMartContext _context;
        private readonly IPasswordHasher<TMember> _hasher;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _env;

        public CMemberServices(dbFurniMartContext context, IPasswordHasher<TMember> hasher, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
        {
            _context = context;
            _hasher = hasher;
            _http = httpContextAccessor;
            _env = env;
        }

        //註冊帳號
        public async Task<ResultDTO> MemberCreateAccountAsync(ReqMemberCreateAccountDTO reqdto, CancellationToken ct = default)
        {
            // 1) 基礎驗證
            if (!Regex.IsMatch(reqdto.Phone ?? "", @"^\d{10}$"))
                throw new InvalidOperationException("手機號碼格式不正確，需為10位數字。");
            if (string.IsNullOrWhiteSpace(reqdto.Password) || reqdto.Password.Length < 6)
                throw new InvalidOperationException("密碼長度至少 6 碼。");
            if (string.IsNullOrWhiteSpace(reqdto.Email) || !Regex.IsMatch(reqdto.Email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
                throw new InvalidOperationException("Email 格式不正確。");

            // 2) 唯一性檢查（帳號 / 手機 / Email）
            if (await _context.TMembers.AsNoTracking().AnyAsync(x => x.FAccount == reqdto.Account, ct))
                throw new InvalidOperationException("此帳號已存在，請重新輸入");
            if (await _context.TMembers.AsNoTracking().AnyAsync(x => x.FPhone == reqdto.Phone, ct))
                throw new InvalidOperationException("此手機號碼已存在，請重新輸入");
            if (await _context.TMembers.AsNoTracking().AnyAsync(x => x.FEmail == reqdto.Email, ct))
                throw new InvalidOperationException("此 Email 已存在，請重新輸入");

            var entity = new TMember
            {
                FName = reqdto.Name,
                FPhone = reqdto.Phone,
                FEmail = reqdto.Email,
                FAccount = reqdto.Account,
                //預設
                FMemberImage = "default.png",
                FPhoneState = false,
                FEmailState = false,
                FLeveId = 1,
                FMoneySum = 0,
                FStatus = 1,
                FCreatTime = DateTime.Now,
                FUpdateTime = DateTime.Now
            };

            // 密碼加鹽
            entity.FPasswords = _hasher.HashPassword(entity, reqdto.Password);

            _context.TMembers.Add(entity);
            await _context.SaveChangesAsync(ct);

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
                BirthDate = member.FBirthDate,
                Phone = member.FPhone,
                Email = member.FEmail,       // 若你已新增欄位
                Address = member.FAddress,
                MemberImage = member.FMemberImage,
                LevelId = member.FLeveId,
                MoneySum = member.FMoneySum,
                Status = member.FStatus,
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
                BirthDate = member.FBirthDate,
                Phone = member.FPhone,
                Address = member.FAddress,
                //MemberImage = member.FMemberImage,
                LevelId = member.FLeveId,
                MoneySum = member.FMoneySum,
                Status = member.FStatus,
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
            // 取當前 HttpContext；若為 null 表示非 HTTP 請求環境
            var httpCtx = _http.HttpContext ?? throw new InvalidOperationException("無法取得目前的 HTTP 內容");

            await httpCtx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

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

        //上傳大頭貼
        //public async Task<ResMemberUploadPhotoDTO> MemberUploadPhotoAsync(int memberId, IFormFile file, CancellationToken ct = default)
        //{
        //    if (file == null || file.Length == 0)
        //        throw new InvalidOperationException("未收到檔案");

        //    // 1) 基本限制（大小 2MB，可自行調整）
        //    const long MAX_BYTES = 2 * 1024 * 1024;
        //    if (file.Length > MAX_BYTES)
        //        throw new InvalidOperationException("檔案過大，限制 2MB 以內");

        //    // 2) 副檔名/ContentType 檢查（僅允許常見圖片）
        //    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        //    var okExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        //    if (!okExts.Contains(ext))
        //        throw new InvalidOperationException("僅支援 jpg、jpeg、png、webp 格式");

        //    var okContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        //    if (!okContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        //        throw new InvalidOperationException("檔案 Content-Type 不正確");

        //    // 3) 準備資料夾與檔名
        //    var folder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
        //                              "MemberHeadImages");
        //    if (!Directory.Exists(folder))
        //        Directory.CreateDirectory(folder);

        //    // 唯一檔名：memberId_時間戳+隨機碼.ext
        //    var fileName = $"{memberId}_{DateTime.UtcNow.Ticks}_{Guid.NewGuid():N}{ext}";
        //    var fullPath = Path.Combine(folder, fileName);

        //    // 4) 寫檔
        //    using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        //    {
        //        await file.CopyToAsync(stream, ct);
        //    }

        //    // 5) 更新 DB（追蹤狀態）
        //    var member = await _context.TMembers.FirstOrDefaultAsync(m => m.FMemberId == memberId, ct);
        //    if (member == null)
        //    {
        //        // 若找不到，刪掉剛剛寫入的檔案以免殘留
        //        try { System.IO.File.Delete(fullPath); } catch { /* ignore */ }
        //        throw new InvalidOperationException("找不到會員資料");
        //    }

        //    // （可選）刪除舊頭貼檔案（若不是 default.png）
        //    if (!string.IsNullOrWhiteSpace(member.FMemberImage) &&
        //        !string.Equals(member.FMemberImage, "default.png", StringComparison.OrdinalIgnoreCase))
        //    {
        //        var oldPath = Path.Combine(folder, member.FMemberImage);
        //        if (System.IO.File.Exists(oldPath))
        //        {
        //            try { System.IO.File.Delete(oldPath); } catch { /* ignore */ }
        //        }
        //    }

        //    member.FMemberImage = fileName;
        //    member.FUpdateTime = DateTime.Now;
        //    await _context.SaveChangesAsync(ct);

        //    // 6) 回完整網址
        //    var req = _http.HttpContext?.Request;
        //    var baseUrl = req == null
        //        ? ""
        //        : $"{req.Scheme}://{req.Host}";
        //    var url = $"{baseUrl}/MemberHeadImages/{fileName}";

        //    return new ResMemberUploadPhotoDTO
        //    {
        //        Url = url,
        //        FileName = fileName
        //    };
        //}

    }
}
