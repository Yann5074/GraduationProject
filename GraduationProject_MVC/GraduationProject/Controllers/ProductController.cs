using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.ViewModels;
using GraduationProject.Services;

using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    
    public class ProductsController : Controller
    {
        private readonly IProductService _ProductService;
        public ProductsController(IProductService ProductService)
        {
            _ProductService = ProductService;
        }


        [HttpGet]
        public async Task<IActionResult> List(CProductSearchKeywordViewModel vm)
        {
            // 從 Service 拿資料
            var allProducts = await _ProductService.ListAsync();

            // 有關鍵字就過濾
            if (!string.IsNullOrWhiteSpace(vm?.txtKeyword))
            {
                string kw = vm.txtKeyword.Trim();
                allProducts = allProducts
                    .Where(p =>
                        (!string.IsNullOrEmpty(p.Name) && p.Name.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // 把搜尋關鍵字放進 ViewBag
            ViewBag.Keyword = vm?.txtKeyword;

            // 傳給 Razor View (List.cshtml)
            return View(allProducts);
        }



    }
}
