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
        private readonly dbFurniMartContext _db;
        private readonly string _wwwrootPath;
        private readonly IWebHostEnvironment _env;
        public CProductService(dbFurniMartContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
            _wwwrootPath = env.WebRootPath;
        }

        public IEnumerable<CProductDTO> SearchProduct(CProductSearchKeywordViewModel vm)
        {
            string keyword = vm.txtKeyword?.Trim();


            var query = _db.TProducts
                .AsNoTracking()
                .Include(o => o.ProductVariants) // 或 ProductVariant（若是一對一）
                    .ThenInclude(v => v.Color)
                .Include(o => o.ProductAssets)   // 或 ProductAsset
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
                    // 若是一對多，決定要抓哪個價格（例如最低價）
                    // 價格：取最低價
                    Price = o.ProductVariants.Min(v => (decimal?)v.FPrice),

                    // 成本：取最低成本
                    Cost = o.ProductVariants.Min(v => (decimal?)v.FCost),
                })
                .ToList();
            return result;
        }



        public CProductDetailDTO GetProductDetail(int id)
        {
            var p = _db.TProducts
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.PStatus)
                .Include(x => x.ProductVariants).ThenInclude(v => v.Color)
                .Include(x => x.ProductAssets)
                .FirstOrDefault(x => x.FProductId == id); // 同步查詢

            if (p == null) return null;

            return new CProductDetailDTO
            {
                ProductId = p.FProductId,
                Name = p.FName,
                Description = p.FDescription,
                CategoryId = p.FCategoryId,
                CategoryName = p.Category?.FName,
                PStatusId = p.FPstatus,
                PStatusName = p.PStatus?.FPStatusName,
                PriceMin = p.ProductVariants.Any() ? p.ProductVariants.Min(v => (decimal?)v.FPrice) : null,
                PriceMax = p.ProductVariants.Any() ? p.ProductVariants.Max(v => (decimal?)v.FPrice) : null,
                CostMin = p.ProductVariants.Any() ? p.ProductVariants.Min(v => (decimal?)v.FCost) : null,
                CostMax = p.ProductVariants.Any() ? p.ProductVariants.Max(v => (decimal?)v.FCost) : null,
                Colors = p.ProductVariants.Where(v => v.Color != null)
                                                .Select(v => v.Color.FColorName)
                                                .Distinct()
                                                .ToList(),
                Images = p.ProductAssets.OrderByDescending(a => a.FIsPrimary)
                                              .ThenBy(a => a.FSortOrder)
                                              .Select(a => new CProductImageDTO
                                              {
                                                  Url = a.FUrl,
                                                  IsPrimary = a.FIsPrimary,
                                                  SortOrder = a.FSortOrder
                                              })
                                              .ToList()
            };
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

        public int Create(CProductCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.Variants == null || dto.Variants.Count == 0)
                throw new InvalidOperationException("至少需要一個規格/型號。");

            using (var tx = _db.Database.BeginTransaction())
            {
                // tProduct
                var product = new TProduct
                {
                    FName = dto.Name,
                    FCategoryId = dto.CategoryId,
                    FWarrantyMonth = dto.WarrantyMonth,
                    FDescription = dto.Description,
                    FAssemblyRequired = dto.AssemblyRequired,
                    FAssemblyPart = dto.AssemblyPart,
                    FDiscount = dto.Discount ?? 0,
                    FPstatus = dto.PStatus
                };
                _db.TProducts.Add(product);
                _db.SaveChanges(); // 拿到 fProductId

                // Variants（SKU 防重）
                foreach (var v in dto.Variants)
                {
                    if (_db.TProductVariants.Any(x => x.FSku == v.SKU))
                        throw new InvalidOperationException($"SKU 重複：{v.SKU}");

                    _db.TProductVariants.Add(new TProductVariant
                    {
                        FProductId = product.FProductId,
                        FSku = v.SKU,
                        FPrice = v.Price,
                        FCost = v.Cost,
                        FStock = v.Stock,
                        FPstatus = v.PStatus ?? dto.PStatus,
                        FColorId = v.ColorId,
                        FLength = v.Length,
                        FWidth = v.Width,
                        FHeight = v.Height,
                        FSizeLabel = v.SizeLabel,
                        FWeight = v.Weight
                    });
                }
                _db.SaveChanges();

                // Assets（檔案存到 wwwroot/images；fUrl 一律以 "/" 開頭）
                if (dto.Assets != null && dto.Assets.Count > 0)
                {
                    var imagesRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "images");
                    if (!Directory.Exists(imagesRoot)) Directory.CreateDirectory(imagesRoot);

                    bool primaryGiven = dto.Assets.Any(a => a.IsPrimary);
                    int orderSeed = 0;

                    foreach (var a in dto.Assets)
                    {
                        if (a.Upload == null || a.Upload.Length == 0) continue;

                        var ext = Path.GetExtension(a.Upload.FileName);
                        var fileName = $"{Guid.NewGuid():N}{ext}";
                        var absPath = Path.Combine(imagesRoot, fileName);
                        using (var stream = File.Create(absPath))
                        {
                            a.Upload.CopyTo(stream); // 同步寫檔
                        }

                        var url = "/images/" + fileName; // 重要：以 "/" 開頭
                        _db.TProductAssets.Add(new TProductAsset
                        {
                            FProductId = product.FProductId,
                            FPicture = fileName, // 檔名
                            FAssetType = a.AssetType,
                            FMimeType = string.IsNullOrWhiteSpace(a.MimeType) ? a.Upload.ContentType : a.MimeType,
                            FUrl = url,          // 站內路徑
                            FMaterialId = a.MaterialId,
                            FTexturedId = a.TexturedId,
                            FModelId = a.ModelId,
                            FPosterUrl = a.PosterUrl,
                            FIsPrimary = primaryGiven ? a.IsPrimary : false,
                            FSortOrder = a.SortOrder ?? orderSeed++
                        });
                    }

                    if (!primaryGiven)
                    {
                        var first = _db.TProductAssets.Local
                            .Where(e => e.FProductId == product.FProductId)
                            .OrderBy(e => e.FSortOrder)
                            .FirstOrDefault();
                        if (first != null) first.FIsPrimary = true;
                    }

                    _db.SaveChanges();
                }

                tx.Commit();
                return product.FProductId;
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
