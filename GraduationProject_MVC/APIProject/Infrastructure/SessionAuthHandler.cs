using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ApiProject.Dictionary;

namespace ApiProject.Infrastructure
{
    // 自訂 Authentication Handler，讓 [Authorize] 可用 Session 驗證
    public class SessionAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IHttpContextAccessor _http;

        public SessionAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IHttpContextAccessor http) : base(options, logger, encoder, clock)
        {
            _http = http;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var context = _http.HttpContext!;
            var session = context.Session;

            // 看 Session 是否有存登入資訊（你之前在登入時寫的那些 key）
            var memberId = session.GetString(CMemberDictionary.SK_LOGIN_ID);
            var account = session.GetString(CMemberDictionary.SK_LOGIN_ACCOUNT);
            var displayName = session.GetString(CMemberDictionary.SK_LOGIN_NAME);

            if (string.IsNullOrEmpty(memberId))
            {
                // ❌ 沒登入
                return Task.FromResult(AuthenticateResult.Fail("No session"));
            }

            // ✅ 登入成功，建立 ClaimsPrincipal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, memberId),
                new Claim(ClaimTypes.Name, account ?? ""),
                new Claim("displayName", displayName ?? "")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}

