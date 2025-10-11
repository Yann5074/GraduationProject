using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
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
        private readonly dbFurniMartContext _db;

        public ProductsController(IProductService ProductService, IWebHostEnvironment env, dbFurniMartContext db)
        {
            _ProductService = ProductService;
            _env = env;
            _db = db;
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

        [HttpGet]
        public IActionResult Create()
        {
            var vm = new CProductCreateViewModel
            {
                CategoryOptions = _db.TCategories
                    .OrderBy(c => c.FSortOrder)
                    .Select(c => new SelectListItem { Value = c.FCategoryId.ToString(), Text = c.FName }).ToList(),
                PStatusOptions = _db.TPstatuses
                    .Select(s => new SelectListItem { Value = s.FPstatus.ToString(), Text = s.FPstatusName }).ToList(),
                ColorOptions = _db.TColors
                    .Select(c => new SelectListItem { Value = c.FColorId.ToString(), Text = c.FColorName }).ToList()
            };

            vm.Product.Variants.Add(new CProductVariantDto { PStatus = 1 });
            vm.Product.Assets.Add(new CProductAssetDto { IsPrimary = true });

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CProductCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.CategoryOptions = _db.TCategories.OrderBy(c => c.FSortOrder)
                    .Select(c => new SelectListItem { Value = c.FCategoryId.ToString(), Text = c.FName }).ToList();
                vm.PStatusOptions = _db.TPstatuses
                    .Select(s => new SelectListItem { Value = s.FPstatus.ToString(), Text = s.FPstatusName }).ToList();
                vm.ColorOptions = _db.TColors
                    .Select(c => new SelectListItem { Value = c.FColorId.ToString(), Text = c.FColorName }).ToList();
                return View(vm);
            }

            var id = _ProductService.Create(vm.Product);
            TempData["Msg"] = $"已新增商品（ID={id}）。";
            return RedirectToAction(nameof(Detail), new { id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {
            if (id == null) return RedirectToAction("List");

            var prod = _db.TProducts.FirstOrDefault(p => p.FProductId == id.Value);
            if (prod == null) return RedirectToAction("List");

            // 軟刪除
            prod.FPstatus = 4;                   // 4 = 刪除/下架（依你的定義）
            prod.FUpdateTime = DateTime.Now;

            _db.SaveChanges();

            TempData["Msg"] = "商品已刪除（軟刪除）。";
            return RedirectToAction("List");
        }
    }

}
