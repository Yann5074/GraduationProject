using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _svc;
        public EmployeeController(IEmployeeService svc) => _svc = svc;

        public async Task<IActionResult> List(CEmplKeywordViewModel vm, CancellationToken ct)
        {
            //呼叫 service 取得資料清單
            //把 vm.Keyword 傳進去當搜尋關鍵字，並把 ct 傳遞下去，整條查詢可被取消
            var items = await _svc.GetEmployeeListAsync(vm.Keyword, ct);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create(CancellationToken ct)
        {
            return View(); // 回傳空白表單
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CEmployeeCreateViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new CEmployeeCreateDTO
            {
                FName = vm.FName,
                FPhone = vm.FPhone,
                FEmail = vm.FEmail,
                FGender = vm.FGender,
                FBloodType = vm.FBloodType,
                FHireDate = vm.FHireDate,
                FRoleId = vm.FRoleId,
                FStatusId = vm.FStatusId,
                FAccount = vm.FAccount,
                FPasswords = vm.FPasswords,
                HeadShotFileName = "default.png"
            };

            try
            {
                var newId = await _svc.CreateEmployeeAsync(dto, ct);
                // 你也可以 TempData 成功訊息
                return RedirectToAction(nameof(List));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(vm.FAccount), ex.Message);
                return View(vm);
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
