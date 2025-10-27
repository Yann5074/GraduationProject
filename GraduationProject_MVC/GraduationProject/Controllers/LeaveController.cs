using GraduationProject.DTOs;
using GraduationProject.Filter;
using GraduationProject.Interfaces;
using GraduationProject.Services;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class LeaveController : SuperController
    {
        private readonly ILeaveService _svc;
        private readonly IUserContextService _user;
        private readonly IWebHostEnvironment _env;
        public LeaveController(ILeaveService svc, IUserContextService user, IWebHostEnvironment env)
        {
            _svc = svc;
            _user = user;
            _env = env;
        }

        //List
        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            var empId = _user.GetEmployeeId();
            if (empId is null) return RedirectToAction("Login", "Home");

            var rows = await _svc.GetMyLeavesAsync(empId.Value, ct);
            var vm = rows.Select(x => new CLeaveListItemViewModel
            {
                LeaveId = x.LeaveId,
                LeaveType = x.LeaveType,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                StatusId = x.StatusId,
                Status = x.StatusName,
                Description = x.Description,
                PictureUrl = string.IsNullOrEmpty(x.PictureFileName)
                    ? null
                    : Url.Content($"~/LeaveEvidence/{x.PictureFileName}"),
                CreateTime = x.CreateTime
            }).ToList();

            return View(vm);
        }

        //Deleted List
        [HttpGet]
        public async Task<IActionResult> DeletedList(CancellationToken ct)
        {
            var empId = _user.GetEmployeeId();
            if (empId is null) return RedirectToAction("Login", "Home");

            var rows = await _svc.GetMyDeletedLeavesAsync(empId.Value, ct);
            var vm = rows.Select(x => new CLeaveListItemViewModel
            {
                LeaveId = x.LeaveId,
                LeaveType = x.LeaveType,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                StatusId = x.StatusId,
                Status = x.StatusName,
                Description = x.Description,
                PictureUrl = string.IsNullOrEmpty(x.PictureFileName)
                    ? null
                    : Url.Content($"~/LeaveEvidence/{x.PictureFileName}"),
                CreateTime = x.CreateTime
            }).ToList();

            return View(vm);
        }

        //Create
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new CLeaveCreateViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CLeaveCreateViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var empId = _user.GetEmployeeId();
            if (empId is null) return RedirectToAction("Login", "Home");

            // 存證明檔
            string? fileName = null;
            if (vm.Picture is { Length: > 0 })
            {
                var ext = Path.GetExtension(vm.Picture.FileName).ToLowerInvariant();
                var allow = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
                if (!allow.Contains(ext))
                {
                    ModelState.AddModelError(nameof(vm.Picture), "僅支援 jpg/png/webp/pdf");
                    return View(vm);
                }

                fileName = $"{Guid.NewGuid():N}{ext}";
                var dir = Path.Combine(_env.WebRootPath, "LeaveEvidence");
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, fileName);
                await using var fs = System.IO.File.Create(path);
                await vm.Picture.CopyToAsync(fs, ct);
            }

            var dto = new CLeaveCreateDTO
            {
                EmployeeId = empId.Value,
                LeaveType = vm.LeaveType,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                Description = vm.Description,
                PictureFileName = fileName
            };

            var newId = await _svc.CreateAsync(dto, ct);
            return RedirectToAction(nameof(List));
        }

        // 主管審核清單
        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> Pending(CancellationToken ct)
        {
            var pending = await _svc.GetPendingAsync(ct);
            var vm = new CLeaveRequestItemViewModel
            {
                LeaveRequests = pending,
                TotalRequests = pending.Count()
            };
            return View(vm);
        }

        // 核准
        [HttpPost]
        [AdminOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, CancellationToken ct)
        {
            var me = _user.GetEmployeeId();
            if (me is null) return Forbid();

            var ok = await _svc.ApproveAsync(id, me.Value, ct);
            return RedirectToAction(nameof(Pending));
        }

        // 駁回
        [HttpPost]
        [AdminOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reason, CancellationToken ct)
        {
            var me = _user.GetEmployeeId();
            if (me is null) return Forbid();

            var ok = await _svc.RejectAsync(id, me.Value, reason, ct);
            return RedirectToAction(nameof(Pending));
        }

        //軟刪
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int leaveId, CancellationToken ct)
        {
            try
            {
                await _svc.SoftDeletedAsync(leaveId, ct);
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                // 處理錯誤
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Error"); // 顯示錯誤頁面
            }
        }

        //硬刪
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id, CancellationToken ct)
        {
            try
            {
                await _svc.DeleteLeavesAsync(id, ct);
                return RedirectToAction("DeletedList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Error");
            }
        }
    }
}
