using GraduationProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class MemberController : Controller
    {
        //DI 測試
        dbFurniMartContext _context;
        public MemberController(dbFurniMartContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult List() { 
            return View();
        }

    }
}
