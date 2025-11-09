using GraduationProject.Dictionary;
using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GraduationProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthService _auth;
        private readonly IEmployeeEmailSender _mail;
        public HomeController(ILogger<HomeController> logger, IAuthService auth, IEmployeeEmailSender mail)
        {
            _logger = logger;
            _auth = auth;
            _mail = mail;
        }

        //登入
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

            // 讀session
            var json = HttpContext.Session.GetString(CEmployeeDictionary.SK_LOGINED_USER);
            var user = json is null ? null : JsonSerializer.Deserialize<SessionUser>(json);

            return RedirectToAction("OrderDashboard", "Analytics");
        }

        //登出
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();               // 清掉登入資料
            return RedirectToAction("Login", "Home");  // 回登入頁
        }

        //忘記密碼
        [HttpGet]
        public IActionResult ForgotPassword(CForgotPasswordViewModel vm)
        {
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(CForgotPasswordViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var token = await _auth.GenerateResetTokenAsync(vm.Account, vm.Email, ct);

            if (token is not null)
            {
                //重設連結
                var url = Url.Action(nameof(ResetPassword), "Home",
                    new { account = vm.Account, token }, Request.Scheme)!;

                var html = $@"
                <p>您好，請點擊以下連結重設密碼（60 分鐘內有效）：</p>
                <p><a href=""{url}"">重設密碼</a></p>
                <p>若非本人操作，請立即檢查帳戶並採取必要保護措施。</p>";

                await _mail.SendAsync(vm.Email, "重設密碼連結", html, ct);
            }

            return RedirectToAction(nameof(Login));
        }

        // 重設密碼
        [HttpGet]
        public IActionResult ResetPassword(string account, string token)
        {
            return View(new CResetPasswordViewModel { Account = account, Token = token });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(CResetPasswordViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            if (!Regex.IsMatch(vm.NewPassword ?? "",
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).{8,}$"))
            {
                ModelState.AddModelError(nameof(vm.NewPassword),
                    "密碼強度不足：至少 8 碼，需包含大小寫字母、數字與符號。");
                return View(vm);
            }

            var ok = await _auth.ResetPasswordAsync(vm.Account, vm.Token, vm.NewPassword, ct);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "重設連結無效或已過期。");
                return View(vm);
            }

            return RedirectToAction(nameof(Login));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
