using GraduationProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class EmployeeController : Controller
    {
        dbFurniMartContext _context;
        public EmployeeController(dbFurniMartContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
