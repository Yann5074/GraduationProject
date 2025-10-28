using GraduationProject.Data;
using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CProductService> _logger;
        private readonly SkuGenerator _skuGenerator;

        public CProductService(
            dbFurniMartContext db,
            IWebHostEnvironment env,
            ILogger<CProductService> logger,
            SkuGenerator skuGenerator)
        {
            _db = db;
            _env = env;
            _logger = logger;
            _skuGenerator = skuGenerator;
        }

        public IEnumerable<CProductDTO> SearchProduct(CProductSearchKeywordViewModel vm)
        {
            string keyword = vm.txtKeyword?.Trim();

            var query = _db.TProducts.AsNoTracking()
                .Include(o => o.ProductVariants).ThenInclude(v => v.Color)
                .Include(o => o.ProductAssets)
                .Include(o => o.FCategory)
                .Include(o => o.FPstatusNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(o =>
                    EF.Functions.Like(o.FName, $"%{keyword}%") ||
                    EF.Functions.Like(o.FCategory.FName, $"%{keyword}%") ||
                    EF.Functions.Like(o.FPstatusNavigation.FPstatusName, $"%{keyword}%") ||
                    o.ProductVariants.Any(v => v.Color != null &&
                        EF.Functions.Like(v.Color.FColorName, $"%{keyword}%")));
            }

            var result = query.Select(o => new CProductDTO
            {
                ProductId = o.FProductId,
                Name = o.FName,
                Description = o.FDescription,
                CategoryId = o.FCategoryId,
                CategoryName = o.FCategory.FName,
                FPStatusName = o.FPstatusNavigation.FPstatusName,
                ColorName = string.Join(", ", o.ProductVariants.Where(v => v.Color != null)
                                     .Select(v => v.Color.FColorName).Distinct()),
                Price = o.ProductVariants.Any() ? o.ProductVariants.Min(v => v.FPrice) : null,
                Cost = o.ProductVariants.Any() ? o.ProductVariants.Min(v => v.FCost) : null,
                PrimaryImageUrl = o.ProductAssets.Where(a => a.FIsPrimary == true).Select(a => a.FUrl)
                                     .FirstOrDefault() ?? "/ProductImages/default.png",
                ImageUrls = o.ProductAssets.Any()
                    ? o.ProductAssets.OrderBy(a => a.FSortOrder).Select(a => a.FUrl).ToList()
                    : new List<string> { "/ProductImages/default.png" },
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
                    PStatus = v.FPstatus,
                    PStatusName = _db.TPstatuses.Where(s => s.FPstatus == (v.FPstatus ?? -1))
                                      .Select(s => s.FPstatusName).FirstOrDefault() ?? "未知"
                }).ToList()
            }).ToList();

            return result;
        }

        public CProductDetailDTO? GetDetail(int productId)
        {
            var p = _db.TProducts
                .Include(x => x.ProductVariants).ThenInclude(v => v.Color)
                .Include(x => x.ProductAssets)
                .FirstOrDefault(x => x.FProductId == productId);

            if (p == null) return null;

            return new CProductDetailDTO
            {
                ProductId = p.FProductId,
                Name = p.FName!,
                CategoryId = p.FCategoryId,
                Description = p.FDescription,
                PStatus = p.FPstatus,
                WarrantyMonth = p.FWarrantyMonth,
                AssemblyRequired = p.FAssemblyRequired,
                AssemblyPart = p.FAssemblyPart,
                Discount = p.FDiscount,


                ImageUrls = p.ProductAssets.OrderBy(a => a.FSortOrder).Select(a => a.FUrl).ToList(),


                Assets = p.ProductAssets
                    .OrderBy(a => a.FSortOrder)
                    .Select(a => new CProductAssetDTO
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
                    }).ToList(),

                Variants = p.ProductVariants.Select(v => new CProductVariantDTO
                {
                    VariantId = v.FProductVariantId,
                    SKU = v.FSku,
                    ColorId = v.FColorId,
                    ColorName = v.Color?.FColorName,
                    ColorCode = v.Color?.FColorCode,
                    Price = v.FPrice,
                    Cost = v.FCost,
                    Stock = v.FStock,
                    SizeLabel = v.FSizeLabel,
                    Length = v.FLength,
                    Width = v.FWidth,
                    Height = v.FHeight,
                    Weight = v.FWeight,
                    PStatus = v.FPstatus,
                    PStatusName = _db.TPstatuses.Where(s => s.FPstatus == (v.FPstatus ?? -1))
                                      .Select(s => s.FPstatusName).FirstOrDefault() ?? "未知"
                }).ToList()
            };
        }


        public (int ProductId, Dictionary<int, int> VariantIdMap) CreateProductAndVariants(CProductUpdateDTO dto)
        {
            var now = DateTime.Now;

            var p = new TProduct
            {
                FName = dto.Name,
                FCategoryId = dto.CategoryId,
                FDescription = dto.Description,
                FPstatus = dto.PStatus,
                FWarrantyMonth = dto.WarrantyMonth,
                FAssemblyRequired = dto.AssemblyRequired,
                FAssemblyPart = dto.AssemblyPart,
                FDiscount = dto.Discount,
                FCreateTime = now,
                FUpdateTime = now
            };
            _db.TProducts.Add(p);
            _db.SaveChanges(); // 取得 FProductId

            var map = new Dictionary<int, int>(); // tempIndex -> realId

            if (dto.Variants != null && dto.Variants.Count > 0)
            {
                for (int i = 0; i < dto.Variants.Count; i++)
                {
                    var v = dto.Variants[i];
                    if (v == null || v.Deleted == true) continue;

                    // 簡單 SKU 重複檢查（可視情況調整）
                    if (!string.IsNullOrWhiteSpace(v.SKU) && SkuExists(v.SKU, null))
                        throw new InvalidOperationException($"SKU 重複：{v.SKU}");

                    var ev = new TProductVariant
                    {
                        FProductId = p.FProductId,
                        FSku = v.SKU,
                        FColorId = v.ColorId,
                        FPrice = v.Price,
                        FCost = v.Cost,
                        FStock = v.Stock,
                        FSizeLabel = v.SizeLabel,
                        FLength = v.Length,
                        FWidth = v.Width,
                        FHeight = v.Height,
                        FWeight = v.Weight,
                        FPstatus = v.PStatus,
                        FCreateTime = now,
                        FUpdateTime = now
                    };
                    _db.TProductVariants.Add(ev);
                    _db.SaveChanges(); // 取得 FProductVariantId

                    map[i] = ev.FProductVariantId;
                }
            }

            return (p.FProductId, map);
        }

        // ===================== Create：建立 Assets =====================
        public void CreateAssetsForProduct(int productId, List<CProductAssetDTO> assets)
        {
            if (assets == null || assets.Count == 0) return;

            var now = DateTime.Now;
            foreach (var a in assets.Where(x => x != null && x.Deleted != true))
            {
                var entity = new TProductAsset
                {
                    FProductId = productId,
                    FProductVariantId = a.ProductVariantId, // null = 商品層
                    FAssetType = a.AssetType,
                    FMimeType = a.MimeType,
                    FUrl = a.Url,
                    FIsPrimary = a.IsPrimary,
                    FSortOrder = a.SortOrder,
                    FPosterUrl = a.PosterUrl,
                    FMaterialId = a.MaterialId,
                    FTexturedId = a.TexturedId,
                    FModelId = a.ModelId,
                    FMetadateJson = a.MetadateJson,
                    FCreateTime = now,
                    FUpdateTime = now
                };
                _db.TProductAssets.Add(entity);
            }
            _db.SaveChanges();
        }

        // ===================== Create：一步到位（可選） =====================
        public int Create(CProductUpdateDTO dto)
        {
            var (pid, map) = CreateProductAndVariants(dto);

            if (dto.Assets != null)
            {
                foreach (var a in dto.Assets.Where(x => x?.Deleted != true))
                {
                    var tmp = a?.VariantTempIndex ?? -1;
                    if (tmp >= 0 && map.TryGetValue(tmp, out var vid))
                        a.ProductVariantId = vid;
                    else
                        a.ProductVariantId = null;
                }
                CreateAssetsForProduct(pid, dto.Assets);
            }

            return pid;
        }

        // ===================== Upload：同步版 =====================
        public (string url, string? mime) UploadAsset(IFormFile file)
        {
            if (file == null || file.Length == 0) throw new InvalidOperationException("Empty file.");

            var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var folder = Path.Combine(webRoot, "ProductImages");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".bin";
            var name = $"{Guid.NewGuid():N}{ext}";
            var full = Path.Combine(folder, name);

            using (var fs = new FileStream(full, FileMode.CreateNew))
            {
                file.CopyTo(fs);
            }

            var url = $"/ProductImages/{name}";
            var mime = string.IsNullOrWhiteSpace(file.ContentType) ? null : file.ContentType;
            return (url, mime);
        }

        // ===================== 查詢 / 讀取（僅放 Create 需要的選項） =====================
        public List<SelectListItem> GetCategoryOptions()
        {
            return _db.TCategories
                .OrderBy(c => c.FSortOrder)
                .Select(c => new SelectListItem { Value = c.FCategoryId.ToString(), Text = c.FName })
                .ToList();
        }

        public List<SelectListItem> GetColorOptions()
        {
            return _db.TColors
                .OrderBy(c => c.FColorName)
                .Select(c => new SelectListItem { Value = c.FColorId.ToString(), Text = c.FColorName })
                .ToList();
        }

        public List<SelectListItem> GetPStatusOptions()
        {
            return _db.TPstatuses
                .OrderBy(s => s.FPstatusName)
                .Select(s => new SelectListItem { Value = s.FPstatus.ToString(), Text = s.FPstatusName })
                .ToList();
        }

        public List<SelectListItem> GetTextureOptions()
        {
            return _db.TTextures
                .OrderBy(t => t.FTextureName)
                .Select(t => new SelectListItem { Value = t.FTextureId.ToString(), Text = t.FTextureName })
                .ToList();
        }

        public int Update(CProductUpdateDTO dto)
        {
            var now = DateTime.UtcNow;

            var product = _db.TProducts
                .Include(x => x.ProductVariants)
                .Include(x => x.ProductAssets)
                .FirstOrDefault(x => x.FProductId == dto.ProductId)
                ?? throw new ArgumentException("商品不存在");

            if (dto.PStatus.HasValue && !_db.TPstatuses.Any(s => s.FPstatus == dto.PStatus.Value))
                throw new ArgumentException("指定的商品狀態不存在");
            if (dto.CategoryId.HasValue && !_db.TCategories.Any(c => c.FCategoryId == dto.CategoryId.Value))
                throw new ArgumentException("指定的分類不存在");

            // 確保不為 null，避免後續 NRE
            dto.Variants ??= new List<CProductVariantDTO>();
            dto.Assets ??= new List<CProductAssetDTO>();

            // 先對前端傳入的主圖做群組正規化（每個 scope 僅保留一個 true）
            NormalizePrimaryPerScope(dto.Assets);

            using var tx = _db.Database.BeginTransaction();

            // ===== 主檔 =====
            product.FName = dto.Name;
            product.FCategoryId = dto.CategoryId;
            product.FDescription = dto.Description;
            product.FPstatus = dto.PStatus;
            product.FWarrantyMonth = dto.WarrantyMonth;
            product.FAssemblyRequired = dto.AssemblyRequired;
            product.FAssemblyPart = dto.AssemblyPart;
            product.FDiscount = dto.Discount;
            product.FUpdateTime = now;

            // ===== Variants：刪除 =====
            if (product.ProductVariants != null && product.ProductVariants.Count > 0)
            {
                foreach (var ev in product.ProductVariants.ToList())
                {
                    var incoming = dto.Variants.FirstOrDefault(v => v.VariantId == ev.FProductVariantId);
                    if (incoming?.Deleted == true)
                        _db.TProductVariants.Remove(ev);
                }
            }

            // ===== Variants：新增/更新 =====
            foreach (var v in dto.Variants.Where(x => x.Deleted != true))
            {
                if (v.VariantId.HasValue)
                {
                    var ev = product.ProductVariants.FirstOrDefault(x => x.FProductVariantId == v.VariantId.Value);
                    if (ev != null)
                    {
                        ev.FSku = v.SKU;
                        ev.FPrice = v.Price;
                        ev.FCost = v.Cost;
                        ev.FStock = v.Stock;
                        ev.FPstatus = v.PStatus;
                        ev.FColorId = v.ColorId;
                        ev.FLength = v.Length;
                        ev.FWidth = v.Width;
                        ev.FHeight = v.Height;
                        ev.FSizeLabel = v.SizeLabel;
                        ev.FWeight = v.Weight;
                        ev.FUpdateTime = now;
                    }
                }
                else
                {
                    _db.TProductVariants.Add(new TProductVariant
                    {
                        FProductId = product.FProductId,
                        FSku = v.SKU,
                        FPrice = v.Price,
                        FCost = v.Cost,
                        FStock = v.Stock,
                        FPstatus = v.PStatus,
                        FColorId = v.ColorId,
                        FLength = v.Length,
                        FWidth = v.Width,
                        FHeight = v.Height,
                        FSizeLabel = v.SizeLabel,
                        FWeight = v.Weight,
                        FCreateTime = now,
                        FUpdateTime = now
                    });
                }
            }
            _db.SaveChanges();

            // ===== Assets：刪除 =====
            if (product.ProductAssets != null && product.ProductAssets.Count > 0)
            {
                foreach (var ea in product.ProductAssets.ToList())
                {
                    var incoming = dto.Assets.FirstOrDefault(a => a.AssetId == ea.FAssetId);
                    if (incoming?.Deleted == true)
                        _db.TProductAssets.Remove(ea);
                }
            }

            // ===== Assets：新增/更新（確保 IsPrimary 正確寫入） =====
            foreach (var a in dto.Assets.Where(x => x.Deleted != true))
            {
                if (a.AssetId.HasValue)
                {
                    var ea = product.ProductAssets.FirstOrDefault(x => x.FAssetId == a.AssetId.Value);
                    if (ea != null)
                    {
                        ea.FProductVariantId = a.ProductVariantId;                 // null = 商品層
                        ea.FAssetType = a.AssetType;
                        ea.FMimeType = a.MimeType;
                        ea.FUrl = a.Url;
                        ea.FIsPrimary = a.IsPrimary == true;                // ★ 轉為 bool，避免 null 不生效
                        ea.FSortOrder = a.SortOrder;
                        ea.FPosterUrl = a.PosterUrl;
                        ea.FMaterialId = a.MaterialId;
                        ea.FTexturedId = a.TexturedId;
                        ea.FModelId = a.ModelId;
                        ea.FMetadateJson = a.MetadateJson;
                        ea.FUpdateTime = now;
                    }
                }
                else
                {
                    // 以 URL + scope 去重（可視需求保留/移除）
                    var dup = product.ProductAssets?.FirstOrDefault(x =>
                        x.FUrl == a.Url && ((x.FProductVariantId ?? 0) == (a.ProductVariantId ?? 0)));

                    if (dup != null)
                    {
                        dup.FAssetType = a.AssetType;
                        dup.FMimeType = a.MimeType;
                        dup.FIsPrimary = a.IsPrimary == true;                   
                        dup.FSortOrder = a.SortOrder;
                        dup.FPosterUrl = a.PosterUrl;
                        dup.FMaterialId = a.MaterialId;
                        dup.FTexturedId = a.TexturedId;
                        dup.FModelId = a.ModelId;
                        dup.FMetadateJson = a.MetadateJson;
                        dup.FUpdateTime = now;
                    }
                    else
                    {
                        _db.TProductAssets.Add(new TProductAsset
                        {
                            FProductId = product.FProductId,
                            FProductVariantId = a.ProductVariantId,                 // null = 商品層
                            FAssetType = a.AssetType,
                            FMimeType = a.MimeType,
                            FUrl = a.Url,
                            FIsPrimary = a.IsPrimary == true,                
                            FSortOrder = a.SortOrder,
                            FPosterUrl = a.PosterUrl,
                            FMaterialId = a.MaterialId,
                            FTexturedId = a.TexturedId,
                            FModelId = a.ModelId,
                            FMetadateJson = a.MetadateJson,
                            FCreateTime = now,
                            FUpdateTime = now
                        });
                    }
                }
            }

            // 先存一次（確保新增資產拿到 FAssetId，以便做唯一主圖處理）
            _db.SaveChanges();

            // ===== 最終保險：以 DB 現況強制每組僅一個主圖 =====
            EnforceSinglePrimaryInDbScope(product);

            _db.SaveChanges();
            tx.Commit();
            return product.FProductId;
        }

        /// <summary>
        /// 對傳入的 DTO 做群組正規化：以 (ProductVariantId??0) 為 scope，每組最多一個 IsPrimary==true。
        /// </summary>
        private static void NormalizePrimaryPerScope(List<CProductAssetDTO> assets)
        {
            var groups = assets.Where(a => a.Deleted != true)
                               .GroupBy(a => a.ProductVariantId ?? 0);
            foreach (var g in groups)
            {
                // 把 true 的候選排序（先 SortOrder、再 AssetId），保留第一個
                var primaries = g.Where(x => x.IsPrimary == true)
                                 .OrderBy(x => x.SortOrder ?? int.MaxValue)
                                 .ThenBy(x => x.AssetId ?? int.MaxValue)
                                 .ToList();

                if (primaries.Count <= 1) continue;

                var keep = primaries.First();
                foreach (var x in primaries.Skip(1))
                    x.IsPrimary = false;
            }
        }

        /// <summary>
        /// 以目前 product 的 DB 實體為準，強制每個 scope 僅一個 FIsPrimary==true。
        /// </summary>
        private static void EnforceSinglePrimaryInDbScope(TProduct product)
        {
            var groups = (product.ProductAssets ?? Enumerable.Empty<TProductAsset>())
                .GroupBy(a => a.FProductVariantId ?? 0);

            foreach (var g in groups)
            {
                

                var keep = g.Where(a => a.FIsPrimary == true)
                            .OrderBy(a => a.FSortOrder ?? int.MaxValue)
                            .ThenBy(a => a.FAssetId)
                            .FirstOrDefault()
                       ?? g.OrderBy(a => a.FSortOrder ?? int.MaxValue)
                            .ThenBy(a => a.FAssetId)
                            .FirstOrDefault();

                bool kept = false;
                foreach (var a in g)
                {
                    if (!kept && a == keep) { a.FIsPrimary = true; kept = true; }
                    else a.FIsPrimary = false;
                }
            }
        }



        public bool SkuExists(string sku, int? excludeVariantId = null)
        {
            var q = _db.TProductVariants.AsQueryable();
            if (excludeVariantId.HasValue)
                q = q.Where(v => v.FProductVariantId != excludeVariantId.Value);
            return q.Any(v => v.FSku == sku);
        }





        public bool Delete(int productId)
        {
            var p = _db.TProducts
                .Include(x => x.ProductVariants)
                .Include(x => x.ProductAssets)
                .FirstOrDefault(x => x.FProductId == productId);
            if (p == null) return false;

            // 視需求：硬刪除 or 軟刪除（例如設 FIsDeleted=1）
            _db.TProductVariants.RemoveRange(p.ProductVariants);
            _db.TProductAssets.RemoveRange(p.ProductAssets);
            _db.TProducts.Remove(p);
            _db.SaveChanges();
            return true;
        }



    }

}
