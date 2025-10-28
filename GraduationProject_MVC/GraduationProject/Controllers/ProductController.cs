using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.Services;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GraduationProject.Controllers
{

    public class ProductsController : SuperController
    {
        
        private readonly IProductService _ProductService;
        private readonly IWebHostEnvironment _env; // 為了存圖片
        private readonly dbFurniMartContext _db;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService ProductService, IWebHostEnvironment env, dbFurniMartContext db, ILogger<ProductsController> logger)
        {
            _ProductService = ProductService;
            _env = env;
            _db = db;
            _logger = logger;
        }
        private List<SelectListItem> GetCategoryOptions() =>
            _db.TCategories.OrderBy(c => c.FSortOrder)
               .Select(c => new SelectListItem { Value = c.FCategoryId.ToString(), Text = c.FName })
               .ToList();

        private List<SelectListItem> GetPStatusOptions() =>
            _db.TPstatuses.OrderBy(s => s.FPstatus)
               .Select(s => new SelectListItem { Value = s.FPstatus.ToString(), Text = s.FPstatusName })
               .ToList();

        private List<SelectListItem> GetColorOptions() =>
            _db.TColors.OrderBy(c => c.FColorName)
               .Select(c => new SelectListItem { Value = c.FColorId.ToString(), Text = c.FColorName })
               .ToList();

        public IActionResult List([FromQuery] CProductSearchKeywordViewModel vm)
        {
            var data = _ProductService.SearchProduct(vm);


            ViewBag.Keyword = vm.txtKeyword;

            return View(data);
        }



        public IActionResult Detail(int id)
        {
            var dto = _ProductService.GetDetail(id);
            if (dto == null) return NotFound();
            return View(new CProductDetailViewModel { Data = dto });
        }

        [HttpGet]
        public IActionResult Create()
        {
            var vm = BuildCreateViewModel(new CProductUpdateDTO());
            // 至少給一筆空變體，方便直接編輯
            vm.Data.Variants = vm.Data.Variants ?? new List<CProductVariantDTO> { new CProductVariantDTO() };
            vm.Data.Assets = vm.Data.Assets ?? new List<CProductAssetDTO>();
            return View(vm);
        }

        // --------- Create (POST) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind(Prefix = "Data")] CProductUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var vmInvalid = BuildCreateViewModel(dto);
                return View(vmInvalid);
            }

            // (1) 先建 product + variants，取得 tempIndex -> real VariantId 對照
            var (pid, map) = _ProductService.CreateProductAndVariants(dto);

            // (2) 將資產的 ProductVariantId 對齊對應變體
            if (dto.Assets != null)
            {
                foreach (var a in dto.Assets)
                {
                    if (a?.Deleted == true) continue;
                    var tmp = a?.VariantTempIndex ?? -1;
                    if (tmp >= 0 && map.TryGetValue(tmp, out var realVid))
                        a.ProductVariantId = realVid;
                    else
                        a.ProductVariantId = null; // 商品層資產
                }

                _ProductService.CreateAssetsForProduct(pid, dto.Assets);
            }

            TempData["Ok"] = "已建立商品";
            return RedirectToAction(nameof(Edit), new { id = pid });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadAsset(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file.");
            var (url, mime) = _ProductService.UploadAsset(file);
            return Json(new { url, mime });
        }

        // --------- 小工具：建置 Create 用 VM ----------
        private CProductEditViewModel BuildCreateViewModel(CProductUpdateDTO dto)
        {
            return new CProductEditViewModel
            {
                Data = dto,
                CategoryOptions = _ProductService.GetCategoryOptions(),
                PStatusOptions = _ProductService.GetPStatusOptions(),
                ColorOptions = _ProductService.GetColorOptions(),
                TextureOptions = _ProductService.GetTextureOptions()
            };
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var dto = _ProductService.GetDetail(id);
            if (dto == null) return NotFound();

            var vm = new CProductEditViewModel
            {
                Data = new CProductUpdateDTO
                {
                    ProductId = dto.ProductId,
                    Name = dto.Name,
                    CategoryId = dto.CategoryId,
                    Description = dto.Description,
                    PStatus = dto.PStatus,
                    WarrantyMonth = dto.WarrantyMonth,
                    AssemblyRequired = dto.AssemblyRequired,
                    AssemblyPart = dto.AssemblyPart,
                    Discount = dto.Discount,
                    Variants = dto.Variants,
                    Assets = dto.Assets,
                },
                CategoryOptions = GetCategoryOptions(),
                PStatusOptions = GetPStatusOptions(),
                ColorOptions = GetColorOptions()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind(Prefix = "Data")] CProductUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var vm = new CProductEditViewModel
                {
                    Data = dto,
                    CategoryOptions = GetCategoryOptions(),
                    PStatusOptions = GetPStatusOptions(),
                    ColorOptions = GetColorOptions()
                };
                return View(vm);
            }
            _ProductService.Update(dto);
            TempData["Ok"] = "已更新商品";
            return RedirectToAction(nameof(Edit), new { id = dto.ProductId });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (_ProductService.Delete(id))
            {
                TempData["Ok"] = "已刪除商品";
            }
            else
            {
                TempData["Error"] = "刪除失敗或商品不存在";
            }
            return RedirectToAction(nameof(List));
        }



        [HttpPost("UploadAsset")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAsset(IFormFile file, [FromServices] IWebHostEnvironment env)
        {
            if (file == null || file.Length == 0)
                return BadRequest("檔案為空");

            // 僅允許圖片
            if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/"))
                return BadRequest("只允許上傳圖片類型 (image/*)");

            // 大小限制（例如 10MB）
            const long MaxSize = 10L * 1024 * 1024;
            if (file.Length > MaxSize)
                return BadRequest("檔案過大，請小於 10MB");

            // 副檔名白名單（可依實際需求增減）
            var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName);
            if (!allowedExt.Contains(ext))
                return BadRequest("僅允許 png/jpg/jpeg/gif/webp");

            // 目的資料夾：/wwwroot/ProductImages
            var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var saveDir = Path.Combine(webRoot, "ProductImages");
            Directory.CreateDirectory(saveDir);

            // 以時間 + Guid 命名避免重名
            var fname = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(saveDir, fname);

            // 寫檔
            await using (var fs = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(fs);
            }

            // 回傳可供前端填入的 URL 與 mime
            var publicUrl = $"/ProductImages/{fname}";
            return Json(new { url = publicUrl, mime = file.ContentType, name = fname });
        }

    }

}
