using GraduationProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class TestController : Controller
    {
        dbFurniMartContext _context;
        public TestController(dbFurniMartContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
