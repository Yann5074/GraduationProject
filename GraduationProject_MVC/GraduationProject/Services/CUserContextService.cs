using GraduationProject.Dictionary;
using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using System.Text.Json;

namespace GraduationProject.Services
{
    public class CUserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _http;
        public CUserContextService(IHttpContextAccessor http) => _http = http;

        public int GetEmployeeId()
        {
            var json = _http.HttpContext?.Session.GetString(CEmployeeDictionary.SK_LOGINED_USER);
            var user = string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<SessionUser>(json);

            return user.Id;
        }

        public string GetEmployeeName()
        {
            var json = _http.HttpContext?.Session.GetString(CEmployeeDictionary.SK_LOGINED_USER);
            var user = string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<SessionUser>(json);

            return user?.Name ?? user?.Account;
        }

    }
}
