using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Controllers
{
    public class AnnouncementController : SuperController
    {
            private readonly IAnnouncementService _svc;
            public AnnouncementController(IAnnouncementService svc) => _svc = svc;

            // 列表
            public async Task<IActionResult> List(bool? active)
            {
                var data = await _svc.GetAllAsync(active);
                return View(data);
            }

            // 新增
            [HttpGet]
            public IActionResult Create()
            {
                return View(new CAnnouncementViewModel { StartAtLocal = DateTime.Now });
            }
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(CAnnouncementViewModel vm)
            {
                if (!ModelState.IsValid) return View(vm);

                var dto = new CSaveAnnouncementDTO
                {
                    Title = vm.Title,
                    Message = vm.Message,
                    StartAt = vm.StartAtLocal,
                    EndAt = vm.EndAtLocal.HasValue ? vm.EndAtLocal.Value : null,
                    IsActive = vm.IsActive,
                    Priority = vm.Priority,
                    // 若你的 DTO/後端有啟用併發，建立可不帶
                    //RowVersion = vm.RowVersion 
                };

                if (dto.EndAt.HasValue && dto.EndAt < dto.StartAt)
                {
                    ModelState.AddModelError(nameof(vm.EndAtLocal), "結束時間不可早於開始時間");
                    return View(vm);
                }

                await _svc.CreateAsync(dto);
                TempData["ok"] = "已建立公告";
                return RedirectToAction(nameof(List));
            }

            // 編輯
            [HttpGet]
            public async Task<IActionResult> Edit(int id)
            {
                var dto = await _svc.GetAsync(id);
                if (dto is null) return NotFound();

                var vm = new CAnnouncementViewModel
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    Message = dto.Message,
                    StartAtLocal = dto.StartAt,
                    EndAtLocal = dto.EndAt.HasValue ? dto.EndAt.Value : null,
                    IsActive = dto.IsActive,
                    Priority = dto.Priority,
                    // 若有啟用 rowversion
                    //RowVersion = dto.RowVersion
                };
                return View(vm);
            }
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(CAnnouncementViewModel vm)
            {
                if (!ModelState.IsValid) return View(vm);

                var dto = new CSaveAnnouncementDTO
                {
                    Title = vm.Title,
                    Message = vm.Message,
                    StartAt = vm.StartAtLocal,
                    EndAt = vm.EndAtLocal.HasValue ? vm.EndAtLocal.Value : null,
                    IsActive = vm.IsActive,
                    Priority = vm.Priority,
                    //RowVersion = vm.RowVersion
                };

                if (dto.EndAt.HasValue && dto.EndAt < dto.StartAt)
                {
                    ModelState.AddModelError(nameof(vm.EndAtLocal), "結束時間不可早於開始時間");
                    return View(vm);
                }

                try
                {
                    await _svc.UpdateAsync(vm.Id!.Value, dto);
                    TempData["ok"] = "已更新公告";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex) when (
                    ex is ValidationException ||
                    ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ||
                    ex is ArgumentException)
                {
                    ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                    return View(vm);
                }
            }

            // 刪除
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Delete(int id)
            {
                await _svc.DeleteAsync(id);
                TempData["ok"] = "已刪除公告";
                return RedirectToAction(nameof(Index));
            }
    }
}
