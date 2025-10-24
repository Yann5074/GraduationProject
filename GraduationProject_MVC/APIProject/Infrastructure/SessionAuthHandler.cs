using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ApiProject.Dictionary;

namespace ApiProject.Infrastructure
{
    // 讓 [Authorize(AuthenticationSchemes = "SessionAuth")] 或全域預設使用 Session 做驗證
    public class SessionAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public SessionAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        { }

        // ★ 這裡可以是 async，載入 Session
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 1) CORS 預檢請求直接略過，不要擋住
            if (HttpMethods.IsOptions(Request.Method))
                return AuthenticateResult.NoResult();

            // 2) 確認 Session 中介軟體有啟用
            if (Context.Session == null)
                return AuthenticateResult.Fail("Session middleware not enabled.");

            // 3) 載入 Session 後再讀取
            await Context.Session.LoadAsync();

            var memberId = Context.Session.GetString(CMemberDictionary.SK_LOGIN_ID);
            var account = Context.Session.GetString(CMemberDictionary.SK_LOGIN_ACCOUNT) ?? "";
            var displayName = Context.Session.GetString(CMemberDictionary.SK_LOGIN_NAME) ?? "";

            if (string.IsNullOrEmpty(memberId))
                return AuthenticateResult.Fail("No session."); // → 觸發 401

            // 4) 建立 ClaimsPrincipal
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, memberId),
                new Claim(ClaimTypes.Name, account),
                new Claim("displayName", displayName),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
