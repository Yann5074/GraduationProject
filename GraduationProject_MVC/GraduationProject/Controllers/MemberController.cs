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
            // 從 Service 拿資料
            var allMembers = await _MemberService.MemberListAsync();

            // 有關鍵字就過濾
            if (!string.IsNullOrWhiteSpace(vm?.Keyword))
            {
                string kw = vm.Keyword.Trim();
                allMembers = allMembers
                    .Where(p =>
                        (!string.IsNullOrEmpty(p.Name) && p.Name.Contains(kw, StringComparison.OrdinalIgnoreCase))||
                        (!string.IsNullOrEmpty(p.Phone) && p.Phone.Contains(kw, StringComparison.OrdinalIgnoreCase))||
                        (!string.IsNullOrEmpty(p.Address) && p.Address.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // 把搜尋關鍵字放進 ViewBag
            ViewBag.Keyword = vm?.Keyword;

            return View(allMembers);
        }

    }
}
