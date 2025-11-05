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

            // 取得副檔名
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            // 檔案大小限制
            const long MaxImageSize = 10L * 1024 * 1024; // 10MB for images
            const long Max3DModelSize = 100L * 1024 * 1024; // 100MB for 3D models

            // 允許的圖片副檔名
            var allowedImageExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".png", ".jpg", ".jpeg", ".gif", ".webp" };

            // 允許的3D模型副檔名
            var allowed3DModelExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".glb", ".gltf" };

            string saveDir;
            string publicUrlPrefix;

            // 判斷檔案類型
            if (allowedImageExt.Contains(ext))
            {
                // 處理圖片檔案
                if (!string.IsNullOrWhiteSpace(file.ContentType) && !file.ContentType.StartsWith("image/"))
                    return BadRequest("圖片檔案的 MIME 類型不正確");

                if (file.Length > MaxImageSize)
                    return BadRequest("圖片檔案過大，請小於 10MB");

                // 儲存到 ProductImages 資料夾
                var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                saveDir = Path.Combine(webRoot, "ProductImages");
                publicUrlPrefix = "/ProductImages/";
            }
            else if (allowed3DModelExt.Contains(ext))
            {
                // 處理3D模型檔案
                if (file.Length > Max3DModelSize)
                    return BadRequest("3D模型檔案過大，請小於 100MB");

                // 儲存到 ProductModels 資料夾
                var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                saveDir = Path.Combine(webRoot, "ProductModels");
                publicUrlPrefix = "/ProductModels/";
            }
            else
            {
                return BadRequest($"不支援的檔案類型。僅允許：圖片({string.Join(", ", allowedImageExt)}) 或 3D模型({string.Join(", ", allowed3DModelExt)})");
            }

            // 建立目錄
            Directory.CreateDirectory(saveDir);

            // 保留原始檔名
            var originalFileName = Path.GetFileName(file.FileName);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);

            // 處理檔名中的非法字元
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                fileNameWithoutExt = fileNameWithoutExt.Replace(c, '_');
            }

            // 檢查檔名是否已存在，如果存在就加上編號
            var finalFileName = $"{fileNameWithoutExt}{ext}";
            var fullPath = Path.Combine(saveDir, finalFileName);

            int counter = 1;
            while (System.IO.File.Exists(fullPath))
            {
                finalFileName = $"{fileNameWithoutExt}_{counter}{ext}";
                fullPath = Path.Combine(saveDir, finalFileName);
                counter++;
            }

            // 寫檔
            await using (var fs = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(fs);
            }

            var publicUrl = $"{publicUrlPrefix}{finalFileName}";

            // 根據檔案類型設定正確的 MIME type
            string mimeType = file.ContentType;
            if (ext == ".glb")
                mimeType = "model/gltf-binary";
            else if (ext == ".gltf")
                mimeType = "model/gltf+json";

            return Json(new
            {
                url = publicUrl,                      // 實際儲存的 URL
                mime = mimeType,                      // MIME 類型
                name = finalFileName,                 // 實際儲存的檔名
                originalFileName = originalFileName,  // 原始檔名
                fileType = ext.TrimStart('.'),        // 檔案類型
                isImage = allowedImageExt.Contains(ext),  // 是否為圖片
                is3DModel = allowed3DModelExt.Contains(ext)  // 是否為3D模型
            });
        }

    }

}
