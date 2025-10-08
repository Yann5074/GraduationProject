using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Services;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

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
        public IActionResult List(CProductSearchKeywordViewModel vm)
        {
            var query = _ProductService.SearchProduct(vm);
            return View(query);
        }

        [HttpGet]
        public IActionResult Create(int? productId)
        {
            

            var vm = new CProductCreateViewModel
            {
                ProductId = productId.Value
                // 可能還需要載入商品資訊/會員清單等
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CProductCreateViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                var dto = new CProductCreateDTO
                {
                    CategoryId = vm.CategoryId,
                    Name = vm.Name,
                    
                };

                int newProductId = _ProductService.CreateProduct(dto); // 回傳訂單ID
                TempData["createSuccessMessage"] = "訂單建立成功";
                return RedirectToAction("List", new { id = newProductId });
            }
            catch
            {
                TempData["createErrorMessage"] = "訂單建立失敗";
                return View(vm);
            }
        }
    }
}
