using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.Services;
using GraduationProject.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class MemberController : Controller
    {
        //DI 測試
        private readonly IMemberService _MemberService;
        private IWebHostEnvironment _enviro;
        public MemberController(IMemberService MemberService, IWebHostEnvironment enviro)
        {
            _MemberService = MemberService;
            _enviro = enviro;
        }

        [HttpGet]
        public async Task<IActionResult> List(CMemberListKeyeordViewModel vm) 
        {
            var members = await _MemberService.MemberSearchAsync(vm.Keyword);
            ViewBag.Keyword = vm.Keyword;
            return View(members);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CMemberCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CMemberCreateViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                // VM -> DTO
                var dto = new CMemberCreateDTO
                {
                    FName = vm.FName!,
                    FDisplayName = vm.FDisplayName,
                    FGender = vm.FGender,
                    FPhone = vm.FPhone,                   
                    FAddress = vm.FAddress
                };

                var created = await _MemberService.MemberCreateAsync(dto, ct);

                TempData["Success"] = $"新增成功（ID: {created.FMemberId}）。";
                return RedirectToAction("List");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "發生未知錯誤，請稍後再試。");
                return View(vm);
            }
        }

        public IActionResult Delete(int? id) 
        {
            var success = _MemberService.MemberDelete(id);
            if (!success)
            {
                TempData["DeleteErrorMessage"] = "刪除失敗，查無指定的訂單";
                return RedirectToAction("List");
            }
            TempData["DeleteSuccessMessage"] = "刪除訂單成功";
            return RedirectToAction("List");
        }

        public async Task<IActionResult> Edit(int id,CancellationToken ct) 
        {
            var vm =await _MemberService.GetEditMember(id,ct);
            if (vm == null)
            {
                return NotFound();
            }
            //var vm = new CMemberUpdateViewModel
            //{
            //    MemberId = dto.MemberId,
            //    Name = dto.Name,
            //    DisplayName = dto.DisplayName,
            //    Gender = dto.Gender,
            //    Phone = dto.Phone,
            //    Address = dto.Address,
            //    MemberImage = dto.MemberImage,
            //    LeveId = dto.LeveId,
            //    MoneySum = dto.MoneySum,
            //    Status = dto.Status,
            //    CreatTime = dto.CreatTime,
            //    UpdateTime = dto.UpdateTime,
            //    Account = dto.Account,
            //    Passwords = dto.Passwords,
            //};

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CMemberUpdateViewModel vm, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return View(vm);

            string? newFileName = null;
            newFileName = await SaveHeadshotAsync(vm.Photo,vm.MemberImage, ct);
            
            var dto = new CMemberUpdateDTO 
            {
                //MemberId = vm.MemberId,
                Name = vm.Name,
                DisplayName = vm.DisplayName,
                Gender = vm.Gender,
                Phone = vm.Phone,
                Address = vm.Address,
                LeveId = vm.LeveId,
                MoneySum = vm.MoneySum,
                Status = vm.Status,
                CreatTime = vm.CreatTime,
                UpdateTime = vm.UpdateTime,
                //Account = vm.Account,
                //Passwords = vm.Passwords,
                // 保留舊檔名（沒上傳時用）
                MemberImage =newFileName ?? vm.MemberImage
            };

            var result = await _MemberService.MemberEdit(id,dto,ct);
            if (result == false)
            {                
                return RedirectToAction("List");
            }
          
            return RedirectToAction("List");

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
            var dir = Path.Combine(_enviro.WebRootPath, "MemberHeadImages");
            Directory.CreateDirectory(dir);

            // 3) 儲存新檔（非同步）
            var path = Path.Combine(dir, fileName);
            await using (var fs = System.IO.File.Create(path))
            {
                await photo.CopyToAsync(fs, ct);
            }

            return fileName;
        }

    }
}
