using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
