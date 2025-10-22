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

        [HttpGet]
        public IActionResult List(CProductSearchKeywordViewModel vm)
        {
            var data = _ProductService.SearchProduct(vm);


            ViewBag.Keyword = vm.txtKeyword;

            return View(data);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            var vm = new CProductCreateViewModel
            {
                CategoryOptions = _db.TCategories
                    .AsNoTracking()
                    .OrderBy(c => c.FSortOrder)
                    .Select(c => new SelectListItem { Value = c.FCategoryId.ToString(), Text = c.FName })
                    .ToList(),
                PStatusOptions = _db.TPstatuses
                    .AsNoTracking()
                    .Select(s => new SelectListItem { Value = s.FPstatus.ToString(), Text = s.FPstatusName })
                    .ToList()
            };
            return View(vm);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("create")]
        public IActionResult Create(CProductCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                Rebind(vm);
                return View(vm);
            }

            var dto = vm.ToDto();

            var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            if (!string.IsNullOrWhiteSpace(vm.VariantsJson))
                dto.Variants = JsonSerializer.Deserialize<List<CProductVariantDTO>>(vm.VariantsJson!, jsonOpts) ?? new();
            if (!string.IsNullOrWhiteSpace(vm.AssetsJson))
                dto.Assets = JsonSerializer.Deserialize<List<CProductAssetDTO>>(vm.AssetsJson!, jsonOpts) ?? new();
            if (!string.IsNullOrWhiteSpace(vm.PartsJson))
                dto.Parts = JsonSerializer.Deserialize<List<CProductPartDTO>>(vm.PartsJson!, jsonOpts) ?? new();

            var id = _ProductService.Create(dto);
            TempData["Toast"] = "商品已建立，接下來可於編輯頁上傳圖片/設定零件。";
            return RedirectToAction("Edit", new { id });
        }

        private void Rebind(CProductCreateViewModel vm)
        {
            vm.CategoryOptions = _db.TCategories
                .AsNoTracking().OrderBy(c => c.FSortOrder)
                .Select(c => new SelectListItem { Value = c.FCategoryId.ToString(), Text = c.FName }).ToList();

            vm.PStatusOptions = _db.TPstatuses
                .AsNoTracking()
                .Select(s => new SelectListItem { Value = s.FPstatus.ToString(), Text = s.FPstatusName }).ToList();
        }




        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            // 你可以改成用 _service.GetDetail(id) 回來的 DTO，再轉 VM
            var p = _db.TProducts
                .Include(x => x.ProductVariants)
                .Include(x => x.ProductAssets)
                .Include(x => x.ProductParts).ThenInclude(pp => pp.PartColorOptions).ThenInclude(co => co.ColorOptionTextures)
                .FirstOrDefault(x => x.FProductId == id);

            if (p == null) return NotFound();

            var vm = new CProductEditViewModel
            {
                ProductId = p.FProductId,
                Name = p.FName ?? "",
                CategoryId = p.FCategoryId,
                Description = p.FDescription,
                PStatus = p.FPstatus,
                WarrantyMonth = p.FWarrantyMonth,
                AssemblyRequired = p.FAssemblyRequired,
                AssemblyPart = p.FAssemblyPart,
                Discount = p.FDiscount,
                CategoryOptions = _db.TCategories
                    .OrderBy(c => c.FSortOrder)
                    .Select(c => new SelectListItem { Value = c.FCategoryId.ToString(), Text = c.FName })
                    .ToList(),
                PStatusOptions = _db.TPstatuses
                    .Select(s => new SelectListItem { Value = s.FPstatus.ToString(), Text = s.FPstatusName })
                    .ToList(),
            };

            // 將現有子集合序列化為 UpdateDTO 形狀給前端編輯器初始化
            var variants = p.ProductVariants.Select(v => new CProductVariantUpdateDTO
            {
                ProductVariantId = v.FProductVariantId,
                SKU = v.FSku,
                Price = v.FPrice,
                Cost = v.FCost,
                Stock = v.FStock,
                PStatus = v.FPstatus,
                ColorId = v.FColorId,
                Length = v.FLength,
                Width = v.FWidth,
                Height = v.FHeight,
                SizeLabel = v.FSizeLabel,
                Weight = v.FWeight
            }).ToList();

            var assets = p.ProductAssets.Select(a => new CProductAssetUpdateDTO
            {
                AssetId = a.FAssetId,
                ProductVariantId = a.FProductVariantId,
                AssetType = a.FAssetType,
                MimeType = a.FMimeType,
                Url = a.FUrl,
                IsPrimary = a.FIsPrimary,
                SortOrder = a.FSortOrder,
                PosterUrl = a.FPosterUrl,
                MaterialId = a.FMaterialId,
                TexturedId = a.FTexturedId,
                ModelId = a.FModelId,
                MetadateJson = a.FMetadateJson
            }).ToList();

            var parts = p.ProductParts.Select(part => new CProductPartUpdateDTO
            {
                PartId = part.FPartId,
                PartName = part.FPartName ?? "",
                PartCode = part.FPartCode,
                DisplayOrder = part.FDiaplayOrder,
                Options = (part.PartColorOptions ?? new List<TPartColorOption>()).Select(opt => new CPartColorOptionUpdateDTO
                {
                    ColorOptionId = opt.FColorOptionId,
                    OptionName = opt.FOptionName ?? "",
                    ColorHex = opt.FColorHex,
                    Thumbnail = opt.FThumbnail,
                    PriceAdjustment = opt.FPriceAdjustment,
                    IsDefault = opt.FIsDefault,
                    DisplayOrder = opt.FDisplayOrder,
                    Textures = (opt.ColorOptionTextures ?? new List<TColorOptionTexture>()).Select(t => new CColorOptionTextureUpdateDTO
                    {
                        TextureId = t.FTextureId,
                        TextureType = t.FTextureType ?? "",
                        FilePath = t.FFilePath ?? "",
                        Tiling = t.FTiling
                    }).ToList()
                }).ToList()
            }).ToList();

            var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = null }; // 保留大小寫
            vm.VariantsJson = JsonSerializer.Serialize(variants, jsonOpts);
            vm.AssetsJson = JsonSerializer.Serialize(assets, jsonOpts);
            vm.PartsJson = JsonSerializer.Serialize(parts, jsonOpts);

            return View(vm);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("edit/{id:int}")]
        public IActionResult Edit(int id, CProductEditViewModel vm)
        {
            if (id != vm.ProductId) return BadRequest();
            if (!ModelState.IsValid)
            {
                Rebind(vm);
                return View(vm);
            }

            var dto = vm.ToUpdateDto();
            var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (!string.IsNullOrWhiteSpace(vm.VariantsJson))
                dto.Variants = JsonSerializer.Deserialize<List<CProductVariantUpdateDTO>>(vm.VariantsJson!, jsonOpts) ?? new();

            if (!string.IsNullOrWhiteSpace(vm.AssetsJson))
                dto.Assets = JsonSerializer.Deserialize<List<CProductAssetUpdateDTO>>(vm.AssetsJson!, jsonOpts) ?? new();

            if (!string.IsNullOrWhiteSpace(vm.PartsJson))
                dto.Parts = JsonSerializer.Deserialize<List<CProductPartUpdateDTO>>(vm.PartsJson!, jsonOpts) ?? new();

            var pid = _ProductService.Update(dto);
            TempData["Toast"] = "已更新商品";
            return RedirectToAction("Edit", new { id = pid });
        }

        private void Rebind(CProductEditViewModel vm)
        {
            vm.CategoryOptions = _db.TCategories
                .OrderBy(c => c.FSortOrder)
                .Select(c => new SelectListItem { Value = c.FCategoryId.ToString(), Text = c.FName })
                .ToList();
            vm.PStatusOptions = _db.TPstatuses
                .Select(s => new SelectListItem { Value = s.FPstatus.ToString(), Text = s.FPstatusName })
                .ToList();
        }

    }

}
