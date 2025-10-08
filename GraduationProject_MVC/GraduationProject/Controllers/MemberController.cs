using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.Services;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class MemberController : Controller
    {
        //DI 測試
        private readonly IMemberService _MemberService;
        public MemberController(IMemberService MemberService)
        {
            _MemberService = MemberService;
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

    }
}
