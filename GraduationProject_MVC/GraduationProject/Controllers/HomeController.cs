using GraduationProject.Dictionary;
using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using Microsoft.AspNetCore.Authorization;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace GraduationProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthService _auth;
        public HomeController(ILogger<HomeController> logger, IAuthService auth)
        {
            _logger = logger;
            _auth = auth;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(CLoginViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _auth.AuthenticateAsync(vm.txtAccount, vm.txtPassword, ct);
            if (!result.Success || result.User is null)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "帳號或密碼有誤");
                return View(vm);
            }

            HttpContext.Session.SetString(
               CEmployeeDictionary.SK_LOGINED_USER,
               JsonSerializer.Serialize(result.User)
            );

            var json = HttpContext.Session.GetString(CEmployeeDictionary.SK_LOGINED_USER);
            var user = json is null ? null : JsonSerializer.Deserialize<SessionUser>(json);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
