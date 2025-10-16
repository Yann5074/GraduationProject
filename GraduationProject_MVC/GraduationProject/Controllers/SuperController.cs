using GraduationProject.Dictionary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GraduationProject.Controllers
{
    public class SuperController : Controller
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            if (!HttpContext.Session.Keys.Contains(CEmployeeDictionary.SK_LOGINED_USER))
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    Controller = "Home",
                    action = "Login"
                }));
            }
        }
    }
}
