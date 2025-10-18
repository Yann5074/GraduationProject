using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class SessionAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public SessionAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 從 Session 取出你在登入時寫入的內容
        var idStr = Context.Session.GetString("SK_LOGIN_ID");
        var account = Context.Session.GetString("SK_LOGIN_ACCOUNT");
        var name = Context.Session.GetString("SK_LOGIN_NAME") ?? "";

        if (string.IsNullOrEmpty(idStr) || string.IsNullOrEmpty(account))
            return Task.FromResult(AuthenticateResult.NoResult()); // 視為未登入

        // 組成 Claims → 讓 [Authorize] 與 User 可用
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, idStr),
            new Claim(ClaimTypes.Name, account),
            new Claim("displayName", name)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
