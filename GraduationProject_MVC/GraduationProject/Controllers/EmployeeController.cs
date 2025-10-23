using GraduationProject.Dictionary;
using GraduationProject.DTOs;
using GraduationProject.Filter;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GraduationProject.Controllers
{
    public class EmployeeController : SuperController
    {
        //建構子注入
        private readonly IEmployeeService _svc;
        private readonly IWebHostEnvironment _env;
        public EmployeeController(IEmployeeService svc, IWebHostEnvironment env)
        {
            _svc = svc;
            _env = env;
        }

        //List
        public async Task<IActionResult> List(CEmplKeywordViewModel vm, CancellationToken ct)
        {
            // 正規化分頁參數
            var page = vm.Page <= 0 ? 1 : vm.Page;
            var size = vm.Size <= 0 ? 10 : Math.Min(vm.Size, 100); // 最多 100/頁

            var result = await _svc.GetEmployeeListAsync(vm.Keyword, page, size, ct);

            // 保留搜尋框內容
            ViewBag.Keyword = vm.Keyword;
            ViewBag.Page = result.Page;
            ViewBag.Size = result.PageSize;

            return View(result);
        }

        //DeletedList
        public async Task<IActionResult> DeletedList(CEmplKeywordViewModel vm, CancellationToken ct)
        {
            var page = vm.Page <= 0 ? 1 : vm.Page;
            var size = vm.Size <= 0 ? 10 : Math.Min(vm.Size, 100);

            var items = await _svc.GetEmployeeDeletedListAsync(vm.Keyword, page, size);

            ViewBag.Keyword = vm.Keyword;
            ViewBag.Page = items.Page;
            ViewBag.Size = items.PageSize;
            return View(items);
        }

        //Create
        [HttpGet]
        public IActionResult Create(CancellationToken ct)
        {
            return View(); // 回傳空白表單
        }
        [HttpPost]
        [AdminOnly]
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
                return RedirectToAction(nameof(List));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(vm.FAccount), ex.Message);
                return View(vm);
            }
        }

        //Delete
        [HttpPost]
        [AdminOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            var delete = await _svc.DeleteEmployeeAsync(id);
            return RedirectToAction(nameof(List));
        }

        //ReallyDelete
        [HttpPost]
        [AdminOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReallyDelete(int? id)
        {
            var delete = await _svc.ReallyDeleteEmployeeAsync(id);
            return RedirectToAction(nameof(DeletedList));
        }

        //Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id, bool fromDeleted = false)
        {
            var vm = await _svc.GetEmployeeEditVmAsync(id);
            if (vm is null) return NotFound();
            
            vm.FromDeleted = fromDeleted;
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CEmployeeEditViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(vm);

            //引用方法
            string? newFileName = null;
            newFileName = await SaveHeadshotAsync(vm.Photo, vm.FHeadShot, ct);

            var dto = new CEmployeeEditDTO
            {
                FName = vm.FName,
                FPhone = vm.FPhone,
                FEmail = vm.FEmail,
                FHeadShot = newFileName ?? vm.FHeadShot,
                FGender = vm.FGender,
                FBloodType = vm.FBloodType,
                FHireDate = vm.FHireDate,
                FRoleId = vm.FRoleId,
                FStatusId = vm.FStatusId,
                FAccount = vm.FAccount, 
                FPasswords = string.IsNullOrWhiteSpace(vm.NewPassword) ? null : vm.NewPassword,
                FLoginTime = vm.FLoginTime,
                FChangePasswordTime = vm.FChangePasswordTime
            };

            var Edit_ok = await _svc.EditEmployeeAsync(id, dto, ct);
            if (!Edit_ok) return NotFound();

            return RedirectToAction(nameof(List));
        }

        //儲存照片檔案方法
        private async Task<string?> SaveHeadshotAsync(
        IFormFile? photo, string? oldFileName, CancellationToken ct,
        long maxBytes = 2 * 1024 * 1024)
        {
            if (photo is null || photo.Length == 0) return null;

            // 1) 檢查副檔名/大小
            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext)) throw new InvalidOperationException("只允許上傳 jpg / png / webp。");
            if (photo.Length > maxBytes) throw new InvalidOperationException("檔案過大，請小於 2MB。");

            // 2) 生成檔名 & 目錄
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var dir = Path.Combine(_env.WebRootPath, "HeadShotImages");
            Directory.CreateDirectory(dir);

            // 3) 儲存新檔（非同步）
            var path = Path.Combine(dir, fileName);
            await using (var fs = System.IO.File.Create(path))
            {
                await photo.CopyToAsync(fs, ct);
            }

            return fileName;
        }

        //Details
        [HttpGet]
        public async Task<IActionResult> Details(int id, bool fromDeleted = false, CancellationToken ct = default)
        {
            var dto = await _svc.DetailEmployeeAsync(id);
            if (dto is null) return NotFound();

            var vm = new CEmployeeDetailViewModel
            {
                FEmployeeId = dto.FEmployeeId,
                FHeadShot = $"~/HeadShotImages/{dto.FHeadShot}",
                FName = dto.FName ?? "",
                FEmail = dto.FEmail,
                FPhone = dto.FPhone,
                FGenderName = dto.FGenderName,
                FBloodType = dto.FBloodType,
                FRoleClass = dto.FRoleClass,
                FRoleBadgeClass = dto.FRoleId switch
                {
                    1 => "bg-success",   // 實習生
                    2 => "bg-primary",    // 職員
                    3 => "bg-info",   // 組長
                    4 => "bg-warning", // 管理者
                    _ => "bg-dark" //其他
                },
                FStatus = dto.FStatus,
                FStatusBadgeClass = dto.FStatusId switch
                {
                    1 => "bg-success",   // 在職
                    2 => "bg-secondary",    // 離職
                    3 => "bg-warning",   // 退休
                    4 => "bg-danger", // 註銷
                    _ => "bg-dark" //其他
                },
                FAccount = dto.FAccount,
                FHireDate = dto.FHireDate,
                FLoginTime = dto.FLoginTime,
                FChangePasswordTime = dto.FChangePasswordTime,
                FromDeleted = fromDeleted
            };
            return View(vm);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
