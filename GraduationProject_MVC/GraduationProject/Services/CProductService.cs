using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.ContentModel;
using System.Runtime.Intrinsics.X86;


namespace GraduationProject.Services
{
    public class CProductService : IProductService
    {
        private readonly ILogger<CProductService> _logger;
        private readonly dbFurniMartContext _db;
        private readonly string _wwwrootPath;
        private readonly IWebHostEnvironment _env;
        private readonly SkuGenerator _skuGenerator;


        private const string ImagesBaseVirtual = "/ProductImages/";
        private const string PlaceholderPrimary = "/ProductImages/no-image.png";
        private const string PlaceholderFallback = "/images/no-image.png";
        public CProductService(ILogger<CProductService> logger, dbFurniMartContext db, IWebHostEnvironment env, SkuGenerator skuGenerator)
        {
            _db = db;
            _env = env;
            _wwwrootPath = env.WebRootPath;
            _skuGenerator = skuGenerator;
            _logger = logger;
        }

        public IEnumerable<CProductDTO> SearchProduct(CProductSearchKeywordViewModel vm)
        {
            try
            {
                string keyword = vm.txtKeyword?.Trim();

                var query = _db.TProducts
                    .AsNoTracking()
                    .Include(o => o.ProductVariants)
                        .ThenInclude(v => v.Color)
                    .Include(o => o.ProductAssets)
                    .Include(o => o.Category)
                    .Include(o => o.PStatus)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(o =>
                        EF.Functions.Like(o.FName, $"%{keyword}%") ||
                        EF.Functions.Like(o.Category.FName, $"%{keyword}%") ||
                        EF.Functions.Like(o.PStatus.FPStatusName, $"%{keyword}%") ||
                        o.ProductVariants.Any(v => v.Color != null &&
                            EF.Functions.Like(v.Color.FColorName, $"%{keyword}%"))
                    );
                }

                var result = query
                    .Select(o => new CProductDTO
                    {
                        ProductId = o.FProductId,
                        Name = o.FName,
                        Description = o.FDescription,
                        CategoryId = o.FCategoryId,
                        CategoryName = o.Category.FName,
                        PStatus = o.PStatus.FPStatusName,
                        ColorName = string.Join(", ",
                            o.ProductVariants
                                .Where(v => v.Color != null)
                                .Select(v => v.Color.FColorName)
                                .Distinct()),
                        Price = o.ProductVariants.Any()
                            ? o.ProductVariants.Min(v => v.FPrice)
                            : null,
                        Cost = o.ProductVariants.Any()
                            ? o.ProductVariants.Min(v => v.FCost)
                            : null,

                        PrimaryImageUrl = o.ProductAssets
                        .Where(a => a.FIsPrimary == true)
                        .Select(a => a.FUrl)
                        .FirstOrDefault(),
                        // 加入所有圖片
                        ImageUrls = o.ProductAssets
                        .OrderBy(a => a.FSortOrder)
                        .Select(a => a.FUrl)
                        .ToList(),
                        // 加入變體資料
                        Variants = o.ProductVariants.Select(v => new CProductVariantDTO
                        {
                            VariantId = v.FProductVariantId,
                            SKU = v.FSku,
                            ColorName = v.Color != null ? v.Color.FColorName : "無顏色",
                            ColorCode = v.Color != null ? v.Color.FColorCode : null,
                            Price = v.FPrice,
                            Cost = v.FCost,
                            Stock = v.FStock,
                            SizeLabel = v.FSizeLabel,
                            Length = v.FLength,
                            Width = v.FWidth,
                            Height = v.FHeight,
                            Weight = v.FWeight,
                            PStatus = v.FPstatus.HasValue ?
                                _db.TPstatuses.FirstOrDefault(s => s.FPstatus == v.FPstatus).FPStatusName
                                : "未知"
                        }).ToList()
                    })
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋產品時發生錯誤，關鍵字：{Keyword}", vm.txtKeyword);
                throw;
            }
        }



        public CProductDetailDTO GetProductDetail(int id)
        {
            var p = _db.TProducts
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.PStatus)
                .Include(x => x.ProductVariants).ThenInclude(v => v.Color)
                .FirstOrDefault(x => x.FProductId == id);

            if (p == null) return null;

            var variants = p.ProductVariants ?? new List<TProductVariant>();
            var variantIds = variants.Select(v => v.FProductVariantId).ToList();

            // ★ 把「產品層資產」+「變體層資產」一起抓
            var assets = _db.TProductAssets.AsNoTracking()
                .Where(a => a.FProductId == id
                         || (a.FProductVariantId != null && variantIds.Contains(a.FProductVariantId.Value)))
                .OrderByDescending(a => a.FIsPrimary)
                .ThenBy(a => a.FSortOrder)
                .ToList();

            Console.WriteLine($"Product ID: {id}");
            Console.WriteLine($"Assets count: {assets.Count}");

            var dto = new CProductDetailDTO
            {
                ProductId = p.FProductId,
                Name = p.FName,
                Description = p.FDescription,
                CategoryId = p.FCategoryId,
                CategoryName = p.Category?.FName,
                PStatusId = p.FPstatus,
                PStatusName = p.PStatus?.FPStatusName,

                PriceMin = variants.Any() ? variants.Min(v => (decimal?)v.FPrice) : null,
                PriceMax = variants.Any() ? variants.Max(v => (decimal?)v.FPrice) : null,
                CostMin = variants.Any() ? variants.Min(v => (decimal?)v.FCost) : null,
                CostMax = variants.Any() ? variants.Max(v => (decimal?)v.FCost) : null,

                Colors = variants.Where(v => v.Color != null)
                                 .Select(v => v.Color.FColorName)
                                 .Distinct()
                                 .ToList(),

                WarrantyMonth = p.FWarrantyMonth,
                AssemblyRequired = p.FAssemblyRequired,
                AssemblyPart = p.FAssemblyPart,

                LengthMin = variants.Where(v => v.FLength.HasValue).Min(v => (decimal?)v.FLength),
                LengthMax = variants.Where(v => v.FLength.HasValue).Max(v => (decimal?)v.FLength),
                WidthMin = variants.Where(v => v.FWidth.HasValue).Min(v => (decimal?)v.FWidth),
                WidthMax = variants.Where(v => v.FWidth.HasValue).Max(v => (decimal?)v.FWidth),
                HeightMin = variants.Where(v => v.FHeight.HasValue).Min(v => (decimal?)v.FHeight),
                HeightMax = variants.Where(v => v.FHeight.HasValue).Max(v => (decimal?)v.FHeight),
                WeightMin = variants.Where(v => v.FWeight.HasValue).Min(v => (decimal?)v.FWeight),
                WeightMax = variants.Where(v => v.FWeight.HasValue).Max(v => (decimal?)v.FWeight),

                StockTotal = variants.Any() ? variants.Sum(v => v.FStock ?? 0) : (int?)null,

                Images = assets.Select(a => new CProductImageDTO
                {
                    // ★ 來源欄位優先序：FPicture > FUrl > FPosterUrl
                    Url = !string.IsNullOrWhiteSpace(a.FPicture) ? a.FPicture
                                : (!string.IsNullOrWhiteSpace(a.FUrl) ? a.FUrl : a.FPosterUrl),
                    IsPrimary = a.FIsPrimary,
                    SortOrder = a.FSortOrder
                }).ToList()
            };

            // 由 Service 算好 DisplayUrl
            if (dto.Images != null)
                foreach (var img in dto.Images)
                    img.DisplayUrl = ResolveImageUrl(img?.Url);

            return dto;
        }




        private string ResolveImageUrl(string raw)
        {
            var placeholder = FileExistsUnderWebRoot(PlaceholderPrimary) ? PlaceholderPrimary : PlaceholderFallback;
            if (string.IsNullOrWhiteSpace(raw)) return placeholder;

            // 基本正規化
            var u = raw.Trim().Replace("\\", "/");

            // 1) 外部或 data URI
            if (u.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                u.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return u;

            // 2) 去除 query/fragment
            var cut = u.IndexOfAny(new[] { '?', '#' });
            if (cut >= 0) u = u.Substring(0, cut);

            // ⭐ 不要解碼，保持原始檔名
            // u = Uri.UnescapeDataString(u);

            // 3) 統一路徑格式到 /ProductImages/
            string rel;
            if (u.StartsWith("~/ProductImages/"))
                rel = u.Substring(1); // 移除 ~，保留 /ProductImages/xxx
            else if (u.StartsWith("/ProductImages/", StringComparison.OrdinalIgnoreCase))
                rel = u; // 已經是正確格式
            else if (u.StartsWith("~/images/"))
                rel = "/ProductImages/" + u.Substring(9); // ~/images/ → /ProductImages/
            else if (u.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
                rel = "/ProductImages/" + u.Substring(8); // /images/ → /ProductImages/
            else if (u.StartsWith("~/"))
                rel = "/ProductImages/" + u.Substring(2); // ~/ → /ProductImages/
            else if (u.StartsWith("/"))
                rel = "/ProductImages/" + u.Substring(1); // / → /ProductImages/
            else
                rel = "/ProductImages/" + u; // 純檔名 → /ProductImages/xxx

            // 4) 實體檢查
            return FileExistsUnderWebRoot(rel) ? rel : placeholder;
        }

        private bool FileExistsUnderWebRoot(string rootRelativePath)
        {
            if (string.IsNullOrWhiteSpace(rootRelativePath) || _env?.WebRootPath == null)
                return false;

            var physical = Path.Combine(
                _env.WebRootPath,
                rootRelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
            );

            return System.IO.File.Exists(physical);
        }


        public CProductEditViewModel GetProductForEdit(int id)
        {
            var p = _db.TProducts
              .Include(x => x.Category)
              .Include(x => x.PStatus) // 商品狀態
              .Include(x => x.ProductVariants).ThenInclude(v => v.Color)
              .Include(x => x.ProductAssets)
              .FirstOrDefault(x => x.FProductId == id);
            if (p == null) return null;

            var vm = new CProductEditViewModel
            {
                ProductId = p.FProductId,
                Name = p.FName,
                Description = p.FDescription,
                CategoryId = p.FCategoryId,
                PStatusId = p.FPstatus,

                //商品本體新增欄位（請對應你的欄位名）
                WarrantyMonth = p.FWarrantyMonth,
                AssemblyRequired = p.FAssemblyRequired,
                AssemblyPart = p.FAssemblyPart,

                CategoryOptions = _db.TCategories.AsNoTracking()
                                    .OrderBy(c => c.FName)
                                    .Select(c => new SelectListItem { Value = c.FCategoryId.ToString(), Text = c.FName })
                                    .ToList(),

                PStatusOptions = _db.TPstatuses.AsNoTracking()
                                    .OrderBy(s => s.FPStatusName)
                                    .Select(s => new SelectListItem { Value = s.FPStatus.ToString(), Text = s.FPStatusName })
                                    .ToList(),
            };

            // 變體
            var variantStatusOptions = vm.PStatusOptions.ToList(); // 假設變體用同一張狀態表
            vm.Variants = p.ProductVariants
                .OrderBy(v => v.FProductVariantId)
                .Select(v => new CProductVariantEditItem
                {
                    ProductVariantId = v.FProductVariantId,
                    Price = v.FPrice,
                    Cost = v.FCost,
                    Stock = v.FStock,
                    Length = v.FLength,
                    Width = v.FWidth,
                    Height = v.FHeight,
                    Weight = v.FWeight,
                    PStatusId = v.FPstatus,             //變體狀態
                    PStatusOptions = variantStatusOptions, // 每筆帶同一份選單
                    ColorName = v.Color != null ? v.Color.FColorName : null
                })
                .ToList();

            // 資產
            vm.Assets = p.ProductAssets
                .OrderByDescending(a => a.FIsPrimary).ThenBy(a => a.FSortOrder).ThenBy(a => a.FAssetId)
                .Select(a => new CProductAssetEditItem
                {
                    AssetId = a.FAssetId,
                    FPicture = a.FPicture,
                    IsPrimary = a.FIsPrimary ?? false,
                    SortOrder = a.FSortOrder,
                    AssetType = a.FAssetType
                })
                .ToList();

            return vm;

        }

        public bool UpdateProduct(CProductEditViewModel vm)
        {
            // 1) 取出產品與資產
            var p = _db.TProducts
                .Include(x => x.ProductAssets)
                .FirstOrDefault(x => x.FProductId == vm.ProductId);
            if (p == null) return false;

            p.ProductAssets ??= new List<TProductAsset>();

            // 小工具：統一路徑 => 以 "/" 開頭，去除 "~"，斜線一律 "/"
            static string NormalizeRelPath(string input)
            {
                if (string.IsNullOrWhiteSpace(input)) return null;
                var s = input.Trim().Replace("\\", "/");
                if (s.StartsWith("~/")) s = s.Substring(1);      // "~/xxx" -> "/xxx"
                if (!s.StartsWith("/")) s = "/" + s;             // "xxx" -> "/xxx"
                return s;
            }

            // 2) 更新現有圖片（路徑、排序、主圖）
            if (vm.Assets != null && vm.Assets.Count > 0)
            {
                foreach (var a in vm.Assets)
                {
                    var entity = p.ProductAssets.FirstOrDefault(x => x.FAssetId == a.AssetId);
                    if (entity == null) continue;

                    entity.FPicture = NormalizeRelPath(a.FPicture);
                    entity.FSortOrder = a.SortOrder;
                    entity.FIsPrimary = a.IsPrimary;
                    if (entity.GetType().GetProperty("FUpdateTime") != null)
                        entity.GetType().GetProperty("FUpdateTime")!.SetValue(entity, DateTime.UtcNow);
                }
            }

            // 3) 新上傳圖片：存到 wwwroot/ProductImages，DB 存 "/ProductImages/<檔名>"
            if (vm.NewPictures != null && vm.NewPictures.Count > 0)
            {
                var imagesDir = Path.Combine(_wwwrootPath, "ProductImages");
                Directory.CreateDirectory(imagesDir);

                if (vm.NewPicturesMeta == null) vm.NewPicturesMeta = new List<CProductPictureViewModel>();
                while (vm.NewPicturesMeta.Count < vm.NewPictures.Count)
                    vm.NewPicturesMeta.Add(new CProductPictureViewModel());

                var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                     { ".jpg",".jpeg",".png",".webp",".gif",".bmp" };

                for (int i = 0; i < vm.NewPictures.Count; i++)
                {
                    var file = vm.NewPictures[i];
                    if (file == null || file.Length == 0) continue;

                    var ext = Path.GetExtension(file.FileName);
                    if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
                    if (!allowed.Contains(ext)) continue; // 可改成回傳錯誤

                    var uniqueFileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
                    var fullPath = Path.Combine(imagesDir, uniqueFileName);

                    using (var fs = new FileStream(fullPath, FileMode.Create))
                        file.CopyTo(fs);

                    var meta = vm.NewPicturesMeta[i] ?? new CProductPictureViewModel();

                    var asset = new TProductAsset
                    {
                        FProductId = vm.ProductId,
                        FPicture = "/ProductImages/" + uniqueFileName, // 直接存 "/" 開頭
                        FAssetType = "image",
                        FIsPrimary = meta.IsPrimary,
                        FSortOrder = meta.SortOrder
                    };

                    // 可選：寫入時間欄位
                    var t = asset.GetType();
                    t.GetProperty("FCreateTime")?.SetValue(asset, DateTime.UtcNow);
                    t.GetProperty("FUpdateTime")?.SetValue(asset, DateTime.UtcNow);

                    p.ProductAssets.Add(asset);
                }
            }

            // 4) 路徑全面正規化 + 強制只留一張主圖
            var all = p.ProductAssets
                .OrderBy(x => x.FSortOrder ?? int.MaxValue)
                .ThenBy(x => x.FAssetId)
                .ToList();

            foreach (var a in all)
                a.FPicture = NormalizeRelPath(a.FPicture);

            // 挑選第一順位為主圖（若多個為 true 或全部為 false 都適用）
            var chosen = all.FirstOrDefault(x => x.FIsPrimary == true) ?? all.FirstOrDefault();
            if (chosen != null)
            {
                foreach (var a in all)
                    a.FIsPrimary = (a.FAssetId == chosen.FAssetId);
            }

            // 5) 寫入
            var affected = _db.SaveChanges();
            return affected > 0;
        }

        public (bool Success, string Message, int? ProductId) CreateProduct(CProductCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("DB={Db}; DataSource={Src}",
                      _db.Database.GetDbConnection().Database,
                      _db.Database.GetDbConnection().DataSource);

                var now = DateTime.Now;
                var product = new TProduct
                {
                    FName = dto.Name,
                    FCategoryId = dto.CategoryId,
                    FDescription = dto.Description,
                    FWarrantyMonth = dto.WarrantyMonth,
                    FAssemblyRequired = dto.AssemblyRequired,
                    FAssemblyPart = dto.AssemblyRequired ? dto.AssemblyPart : null,
                    FDiscount = dto.Discount,
                    FPstatus = dto.PStatusId, // 確認不是 4
                    FCreateTime = now,
                    FUpdateTime = now,
                    ProductAssets = new List<TProductAsset>(),
                    ProductVariants = new List<TProductVariant>()
                };

                // 產品主圖/附圖
                if (dto.Images?.Any() == true)
                {
                    int sort = 0;
                    foreach (var file in dto.Images)
                    {
                        var (ok, fileName, err) = SaveImage(file);
                        if (!ok) continue;

                        product.ProductAssets.Add(new TProductAsset
                        {
                            // 不用先知道 ProductId，靠導航關係
                            FPicture = fileName,
                            FUrl = $"/ProductImages/{fileName}",
                            FAssetType = "image",
                            FMimeType = file.ContentType,
                            FIsPrimary = sort == 0,
                            FSortOrder = sort++,
                            FCreateTime = now,
                            FUpdateTime = now
                        });
                    }
                }

                // 變體 + 變體圖
                if (dto.Variants?.Any() == true)
                {
                    foreach (var v in dto.Variants)
                    {
                        var sku = _skuGenerator.GenerateSku(dto.CategoryId, v.ColorId, v.Length, v.Width, v.Height);

                        var variant = new TProductVariant
                        {
                            FSku = sku,
                            FPrice = v.Price,
                            FCost = v.Cost,
                            FStock = v.Stock,
                            FColorId = v.ColorId,
                            FLength = v.Length,
                            FWidth = v.Width,
                            FHeight = v.Height,
                            FSizeLabel = string.IsNullOrWhiteSpace(v.SizeLabel) ? $"{v.Length}x{v.Width}x{v.Height}" : v.SizeLabel,
                            FWeight = v.Weight,
                            FPstatus = v.PStatusId,
                            FCreateTime = now,
                            FUpdateTime = now,
                            // 關鍵：加入到 product 的導航集合
                        };

                        product.ProductVariants.Add(variant);

                        if (v.VariantImages?.Any() == true)
                        {
                            int vsort = 0;
                            foreach (var file in v.VariantImages)
                            {
                                var (ok, fileName, err) = SaveImage(file);
                                if (!ok) continue;

                                // 關鍵：用導航屬性關聯到這個 variant
                                product.ProductAssets.Add(new TProductAsset
                                {
                                    FPicture = fileName,
                                    FUrl = $"/ProductImages/{fileName}",
                                    FPosterUrl = $"/ProductImages/{fileName}",
                                    FAssetType = "image",
                                    FMimeType = file.ContentType,
                                    FIsPrimary = vsort == 0,
                                    FSortOrder = vsort++,
                                    FCreateTime = now,
                                    FUpdateTime = now,
                                    // 如果模型有 ProductVariant 導航屬性，設定它：
                                    ProductVariant = variant
                                });
                            }
                        }
                    }
                }

                _db.TProducts.Add(product);
                var affected = _db.SaveChanges();
                if (affected <= 0)
                    return (false, "SaveChanges() 回傳 0，請檢查連線或交易設定", null);

                return (true, "產品新增成功", product.FProductId);
            }
            catch (DbUpdateException ex)
            {
                return (false, $"DB 更新失敗：{ex.GetBaseException().Message}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProduct failed");
                return (false, $"新增失敗：{ex.Message}", null);
            }

        }
        // 儲存圖片的輔助方法 (同步版本)
        private (bool Success, string FileName, string ErrorMessage) SaveImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return (false, null, "檔案為空");

                // 驗證檔案類型
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return (false, null, "不支援的檔案格式");

                // 驗證檔案大小 (例如：5MB)
                if (file.Length > 5 * 1024 * 1024)
                    return (false, null, "檔案大小不可超過 5MB");

                // 產生唯一檔名
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var uploadPath = Path.Combine(_env.WebRootPath, "ProductImages");

                // 確保資料夾存在
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                // 儲存檔案 (同步)
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return (true, fileName, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"儲存圖片失敗：{ex.Message}");
            }
        }




        public bool Delete(int id)
        {
            using (var tx = _db.Database.BeginTransaction())
            {
                var product = _db.TProducts.Find(id);
                if (product == null) return false;

                // 先抓子資料
                var assets = _db.TProductAssets.Where(a => a.FProductId == id).ToList();
                var variants = _db.TProductVariants.Where(v => v.FProductId == id).ToList();

                // 刪實體檔案（限 /wwwroot/images 底下）
                var webroot = _env.WebRootPath ?? "wwwroot";
                var imagesRoot = Path.Combine(webroot, "images");

                foreach (var a in assets)
                {
                    try
                    {
                        string filePath = null;

                        // 優先用 fPicture（純檔名）
                        if (!string.IsNullOrWhiteSpace(a.FPicture))
                        {
                            filePath = Path.Combine(imagesRoot, a.FPicture);
                        }
                        // 再用 fUrl（/images/xxx）
                        else if (!string.IsNullOrWhiteSpace(a.FUrl))
                        {
                            var url = a.FUrl.Replace('\\', '/').Trim();
                            if (url.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
                            {
                                var relative = url.Substring("/images/".Length);
                                filePath = Path.Combine(imagesRoot, relative);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                            File.Delete(filePath);
                    }
                    catch
                    {
                        // 可記錄 Log；不要中斷刪除流程
                    }
                }

                // 先刪子表再刪主表（若 DB 有級聯可省略 RemoveRange）
                _db.TProductAssets.RemoveRange(assets);
                _db.TProductVariants.RemoveRange(variants);
                _db.TProducts.Remove(product);

                _db.SaveChanges();
                tx.Commit();
                return true;
            }
        }






    }

}
