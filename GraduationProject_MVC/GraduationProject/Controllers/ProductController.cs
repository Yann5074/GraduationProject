using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.Services;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
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
                // 回填下拉與子項選單
                var src = _ProductService.GetProductForEdit(vm.ProductId);
                vm.CategoryOptions = src?.CategoryOptions ?? Enumerable.Empty<SelectListItem>();
                vm.PStatusOptions = src?.PStatusOptions ?? Enumerable.Empty<SelectListItem>();
                // 每個 Variant 的 PStatusOptions
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
            // 準備下拉選單資料
            ViewBag.Categories = GetCategorySelectList();
            ViewBag.Colors = GetColorSelectList();
            ViewBag.PStatus = GetPStatusSelectList();

            return View(new CProductCreateDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CProductCreateDTO dto)
        {
            if (dto.AssemblyRequired && string.IsNullOrWhiteSpace(dto.AssemblyPart))
                ModelState.AddModelError(nameof(dto.AssemblyPart), "勾選需要組裝時，組裝部件為必填");

            // ❶ 額外檢查常見必填（避免進入 Service 後才整筆回滾）
            if (dto.Variants != null)
            {
                for (int i = 0; i < dto.Variants.Count; i++)
                {
                    var v = dto.Variants[i];
                    if (v.PStatusId == null)
                        ModelState.AddModelError($"Variants[{i}].PStatusId", "變體狀態為必填");
                    if (string.IsNullOrWhiteSpace(v.SizeLabel))
                        v.SizeLabel = $"{v.Length}x{v.Width}x{v.Height}";
                }
            }

            // 把所有錯誤打到日誌（或暫時顯示）
            if (!ModelState.IsValid)
            {
                
                ViewBag.Categories = GetCategorySelectList();
                ViewBag.Colors = GetColorSelectList();
                ViewBag.PStatus = GetPStatusSelectList();
                return View(dto);
            }
        

            // 呼叫服務
            var (success, message, productId) = _ProductService.CreateProduct(dto);
            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Detail", new { id = productId }); // 確認你的 Action 名稱是否叫 Detail 或 Details
            }

            // 服務失敗 → 顯示錯誤
            ModelState.AddModelError(string.Empty, message);
            ViewBag.Categories = GetCategorySelectList();
            ViewBag.Colors = GetColorSelectList();
            ViewBag.PStatus = GetPStatusSelectList();
            return View(dto);
        }


        //取得分類下拉選單
        private SelectList GetCategorySelectList()
        {
            var categories = _db.TCategories
                .Where(c => c.FIsActive == true)
                .OrderBy(c => c.FSortOrder)
                .Select(c => new { c.FCategoryId, c.FName })
                .ToList();

            return new SelectList(categories, "FCategoryId", "FName");
        }

        //取得顏色下拉選單
        private SelectList GetColorSelectList()
        {
            var colors = _db.TColors
                .OrderBy(c => c.FColorName)
                .Select(c => new { c.FColorId, c.FColorName })
                .ToList();

            return new SelectList(colors, "FColorId", "FColorName");
        }

        //取得產品狀態下拉選單
        private SelectList GetPStatusSelectList()
        {
            var statuses = _db.TPstatuses
                .Select(s => new { s.FPstatus, s.FPStatusName })
                .ToList();

            return new SelectList(statuses, "FPstatus", "FPStatusName");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {
            if (id == null) return RedirectToAction("List");

            var prod = _db.TProducts.FirstOrDefault(p => p.FProductId == id.Value);
            if (prod == null) return RedirectToAction("List");

            // 軟刪除
            prod.FPstatus = 4;                   // 4 = 刪除
            prod.FUpdateTime = DateTime.Now;

            _db.SaveChanges();

            TempData["Msg"] = "商品已刪除（軟刪除）。";
            return RedirectToAction("List");
        }

    }

}
