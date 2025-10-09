using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Services;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GraduationProject.Controllers
{
    
    public class ProductsController : Controller
    {
        private readonly IProductService _ProductService;
        private readonly IWebHostEnvironment _env; // 為了存圖片

        public ProductsController(IProductService ProductService, IWebHostEnvironment env)
        {
            _ProductService = ProductService;
            _env = env;
        }

        [HttpGet]
        public IActionResult List(CProductSearchKeywordViewModel vm)
        {
            var data = _ProductService.SearchProduct(vm);


            ViewBag.Keyword = vm.txtKeyword;

            return View(data);
        }

        [HttpGet]
        public IActionResult Detail(int id)
        {
            var dto = _ProductService.GetProductDetail(id); // 同步呼叫
            if (dto == null) return NotFound();
            return View(dto);
        }

        // GET: /Products/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _ProductService.GetProductForEdit(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CProductEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // 回填下拉與子項選單（避免回傳變空）
                var src = _ProductService.GetProductForEdit(vm.ProductId);
                vm.CategoryOptions = src?.CategoryOptions ?? Enumerable.Empty<SelectListItem>();
                vm.PStatusOptions = src?.PStatusOptions ?? Enumerable.Empty<SelectListItem>();
                // 每個 Variant 的 PStatusOptions 也補回
                var vOptions = src?.PStatusOptions ?? Enumerable.Empty<SelectListItem>();
                vm.Variants?.ForEach(v => v.PStatusOptions = vOptions);
                return View(vm);
            }

            var ok = _ProductService.UpdateProduct(vm);
            if (!ok)
            {
                ModelState.AddModelError("", "更新失敗或資料不存在。");
                var src = _ProductService.GetProductForEdit(vm.ProductId);
                vm.CategoryOptions = src?.CategoryOptions ?? Enumerable.Empty<SelectListItem>();
                vm.PStatusOptions = src?.PStatusOptions ?? Enumerable.Empty<SelectListItem>();
                var vOptions = src?.PStatusOptions ?? Enumerable.Empty<SelectListItem>();
                vm.Variants?.ForEach(v => v.PStatusOptions = vOptions);
                return View(vm);
            }

            TempData["Success"] = "更新成功";
            return RedirectToAction("List");
        }



    }
}
