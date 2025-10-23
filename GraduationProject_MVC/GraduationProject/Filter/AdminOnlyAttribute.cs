using GraduationProject.Dictionary;
using GraduationProject.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace GraduationProject.Filter
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var http = context.HttpContext;
            var json = http.Session.GetString(CEmployeeDictionary.SK_LOGINED_USER);
            var me = string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<SessionUser>(json);

            // RoleId==4(管理職) 而且在職 StatusId==1 才允許
            if (me?.RoleId != 4 || me?.StatusId != 1)
            {
                // 沒權限 → 403 
                context.Result = new ForbidResult();
            }
        }
    }
}
