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
            try
            {
                string keyword = vm.txtKeyword?.Trim();

                var query = _db.TProducts
                    .AsNoTracking()
                    .Include(o => o.ProductVariants)
                        .ThenInclude(v => v.Color)
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
                        CategoryName = o.FCategory.FName,
                        FPStatusName = o.FPstatusNavigation.FPstatusName,
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
                            .FirstOrDefault() ?? "/ProductImages/default.png",
                        // 加入所有圖片
                        ImageUrls = o.ProductAssets.Any()
                            ? o.ProductAssets
                                .OrderBy(a => a.FSortOrder)
                                .Select(a => a.FUrl)
                                .ToList()
                            : new List<string> { "/ProductImages/default.png" },
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
                            PStatus = v.FPstatus,
                            PStatusName = _db.TPstatuses
                                .Where(s => s.FPstatus == (v.FPstatus ?? -1))  // 用 ?? 避免 null 比較
                                .Select(s => s.FPstatusName)
                                .FirstOrDefault() ?? "未知"
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

        public int Create(CProductCreateDTO dto)
        {
            // 前置驗證
            if (dto.PStatus.HasValue)
            {
                bool exists = _db.TPstatuses.Any(s => s.FPstatus == dto.PStatus.Value);
                if (!exists) throw new ArgumentException("指定的商品狀態不存在");
            }
            if (dto.CategoryId.HasValue)
            {
                bool exists = _db.TCategories.Any(c => c.FCategoryId == dto.CategoryId.Value);
                if (!exists) throw new ArgumentException("指定的分類不存在");
            }

            var now = DateTime.UtcNow;

            var product = new TProduct
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

            using var tx = _db.Database.BeginTransaction();

            _db.TProducts.Add(product);
            _db.SaveChanges(); // 取得 fProductId

            // Variants
            foreach (var v in dto.Variants)
            {
                if (v.PStatus.HasValue)
                {
                    bool ok = _db.TPstatuses.Any(s => s.FPstatus == v.PStatus.Value);
                    if (!ok) throw new ArgumentException("指定的變體狀態不存在");
                }

                var entity = new TProductVariant
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
                };
                _db.TProductVariants.Add(entity);
            }
            _db.SaveChanges();

            // Assets（可掛 product 或 variant）
            foreach (var a in dto.Assets)
            {
                var asset = new TProductAsset
                {
                    FProductId = product.FProductId,
                    FProductVariantId = a.ProductVariantId,
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
                _db.TProductAssets.Add(asset);
            }
            _db.SaveChanges();

            // Parts → Options → Textures
            foreach (var p in dto.Parts)
            {
                var part = new TProductPart
                {
                    FProductId = product.FProductId,
                    FPartName = p.PartName,
                    FPartCode = p.PartCode,
                    FDiaplayOrder = p.DisplayOrder   // 注意資料表拼字
                };
                _db.TProductParts.Add(part);
                _db.SaveChanges(); // 需 fPartId

                foreach (var o in p.Options)
                {
                    var opt = new TPartColorOption
                    {
                        FPartId = part.FPartId,
                        FOptionName = o.OptionName,
                        FColorHex = o.ColorHex,
                        FThumbnail = o.Thumbnail,
                        FPriceAdjustment = o.PriceAdjustment,
                        FIsDefault = o.IsDefault,
                        FDisplayOrder = o.DisplayOrder
                    };
                    _db.TPartColorOptions.Add(opt);
                    _db.SaveChanges(); // 需 fColorOptionId

                    foreach (var t in o.Textures)
                    {
                        var tex = new TColorOptionTexture
                        {
                            FColorOptionId = opt.FColorOptionId,
                            FTextureType = t.TextureType,
                            FFilePath = t.FilePath,
                            FTiling = t.Tiling
                        };
                        _db.TColorOptionTextures.Add(tex);
                    }
                    _db.SaveChanges();
                }
            }

            tx.Commit();
            return product.FProductId;
        }


        public CProductDetailDTO? GetDetail(int productId)
        {
            // 你可以換成自己的查詢 DTO 投影；這裡示意直接讀 Entity
            var p = _db.TProducts
                .Include(x => x.ProductVariants)
                .Include(x => x.ProductAssets)
                .Include(x => x.ProductParts).ThenInclude(pp => pp.PartColorOptions).ThenInclude(co => co.ColorOptionTextures)
                .FirstOrDefault(x => x.FProductId == productId);

            if (p == null) return null;

            // TODO：投影為你的 Detail DTO；此處略。你也可以直接在 Controller 用 Entity 填 VM。
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
                // 以及 Variants/Assets/Parts 的投影...
            };
        }

        public int Update(CProductUpdateDTO dto)
        {
            var now = DateTime.UtcNow;

            var product = _db.TProducts
                .Include(x => x.ProductVariants)
                .Include(x => x.ProductAssets)
                .Include(x => x.ProductParts).ThenInclude(pp => pp.PartColorOptions).ThenInclude(co => co.ColorOptionTextures)
                .FirstOrDefault(x => x.FProductId == dto.ProductId);

            if (product == null) throw new ArgumentException("商品不存在");

            // 可選：驗證 PStatus/Category 是否存在
            if (dto.PStatus.HasValue && !_db.TPstatuses.Any(s => s.FPstatus == dto.PStatus.Value))
                throw new ArgumentException("指定的商品狀態不存在");
            if (dto.CategoryId.HasValue && !_db.TCategories.Any(c => c.FCategoryId == dto.CategoryId.Value))
                throw new ArgumentException("指定的分類不存在");

            using var tx = _db.Database.BeginTransaction();

            // 更新主檔
            product.FName = dto.Name;
            product.FCategoryId = dto.CategoryId;
            product.FDescription = dto.Description;
            product.FPstatus = dto.PStatus;
            product.FWarrantyMonth = dto.WarrantyMonth;
            product.FAssemblyRequired = dto.AssemblyRequired;
            product.FAssemblyPart = dto.AssemblyPart;
            product.FDiscount = dto.Discount;
            product.FUpdateTime = now;

            // === Variants Upsert/Delete ===
            var incomingVariantIds = dto.Variants.Where(v => v.ProductVariantId.HasValue && v.Deleted != true)
                                                 .Select(v => v.ProductVariantId!.Value).ToHashSet();

            // 刪除被標記 Deleted 的，以及前端沒傳回且你要「全取代」的可在這裡處理
            foreach (var ev in product.ProductVariants.ToList())
            {
                var incoming = dto.Variants.FirstOrDefault(v => v.ProductVariantId == ev.FProductVariantId);
                if (incoming?.Deleted == true)
                {
                    _db.TProductVariants.Remove(ev);
                }
            }

            // 新增/更新
            foreach (var v in dto.Variants)
            {
                if (v.Deleted == true) continue;

                if (v.ProductVariantId.HasValue)
                {
                    // 更新
                    var ev = product.ProductVariants.FirstOrDefault(x => x.FProductVariantId == v.ProductVariantId.Value);
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
                    // 新增
                    var nv = new TProductVariant
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
                    };
                    _db.TProductVariants.Add(nv);
                }
            }
            _db.SaveChanges();

            // === Assets Upsert/Delete ===
            foreach (var ea in product.ProductAssets.ToList())
            {
                var incoming = dto.Assets.FirstOrDefault(a => a.AssetId == ea.FAssetId);
                if (incoming?.Deleted == true)
                {
                    _db.TProductAssets.Remove(ea);
                }
            }
            foreach (var a in dto.Assets)
            {
                if (a.Deleted == true) continue;

                if (a.AssetId.HasValue)
                {
                    var ea = product.ProductAssets.FirstOrDefault(x => x.FAssetId == a.AssetId.Value);
                    if (ea != null)
                    {
                        ea.FProductVariantId = a.ProductVariantId;
                        ea.FAssetType = a.AssetType;
                        ea.FMimeType = a.MimeType;
                        ea.FUrl = a.Url;
                        ea.FIsPrimary = a.IsPrimary;
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
                    var na = new TProductAsset
                    {
                        FProductId = product.FProductId,
                        FProductVariantId = a.ProductVariantId,
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
                    _db.TProductAssets.Add(na);
                }
            }
            _db.SaveChanges();

            // === Parts / Options / Textures Upsert/Delete ===
            foreach (var ep in product.ProductParts.ToList())
            {
                var incoming = dto.Parts.FirstOrDefault(p => p.PartId == ep.FPartId);
                if (incoming?.Deleted == true)
                {
                    _db.TProductParts.Remove(ep);   // 會 cascade 刪掉 Options/Textures（你可以確認 FK 設定）
                }
            }
            _db.SaveChanges();

            foreach (var p in dto.Parts.Where(x => x.Deleted != true))
            {
                TProductPart targetPart;
                if (p.PartId.HasValue)
                {
                    targetPart = product.ProductParts.FirstOrDefault(x => x.FPartId == p.PartId.Value)
                                 ?? throw new ArgumentException("零件資料不一致");
                    targetPart.FPartName = p.PartName;
                    targetPart.FPartCode = p.PartCode;
                    targetPart.FDiaplayOrder = p.DisplayOrder;
                }
                else
                {
                    targetPart = new TProductPart
                    {
                        FProductId = product.FProductId,
                        FPartName = p.PartName,
                        FPartCode = p.PartCode,
                        FDiaplayOrder = p.DisplayOrder
                    };
                    _db.TProductParts.Add(targetPart);
                    _db.SaveChanges(); // 取得 PartId
                }

                // Options
                foreach (var eo in targetPart.PartColorOptions?.ToList() ?? Enumerable.Empty<TPartColorOption>())
                {
                    var incoming = p.Options.FirstOrDefault(o => o.ColorOptionId == eo.FColorOptionId);
                    if (incoming?.Deleted == true)
                    {
                        _db.TPartColorOptions.Remove(eo);
                    }
                }
                _db.SaveChanges();

                foreach (var o in p.Options.Where(x => x.Deleted != true))
                {
                    TPartColorOption targetOpt;
                    if (o.ColorOptionId.HasValue)
                    {
                        targetOpt = _db.TPartColorOptions.First(x => x.FColorOptionId == o.ColorOptionId.Value);
                        targetOpt.FOptionName = o.OptionName;
                        targetOpt.FColorHex = o.ColorHex;
                        targetOpt.FThumbnail = o.Thumbnail;
                        targetOpt.FPriceAdjustment = o.PriceAdjustment;
                        targetOpt.FIsDefault = o.IsDefault;
                        targetOpt.FDisplayOrder = o.DisplayOrder;
                    }
                    else
                    {
                        targetOpt = new TPartColorOption
                        {
                            FPartId = targetPart.FPartId,
                            FOptionName = o.OptionName,
                            FColorHex = o.ColorHex,
                            FThumbnail = o.Thumbnail,
                            FPriceAdjustment = o.PriceAdjustment,
                            FIsDefault = o.IsDefault,
                            FDisplayOrder = o.DisplayOrder
                        };
                        _db.TPartColorOptions.Add(targetOpt);
                        _db.SaveChanges();
                    }

                    // Textures
                    var existingTex = _db.TColorOptionTextures.Where(t => t.FColorOptionId == targetOpt.FColorOptionId).ToList();
                    foreach (var et in existingTex)
                    {
                        var incomingTex = o.Textures.FirstOrDefault(t => t.TextureId == et.FTextureId);
                        if (incomingTex?.Deleted == true)
                            _db.TColorOptionTextures.Remove(et);
                    }
                    foreach (var t in o.Textures.Where(x => x.Deleted != true))
                    {
                        if (t.TextureId.HasValue)
                        {
                            var et = _db.TColorOptionTextures.First(x => x.FTextureId == t.TextureId.Value);
                            et.FTextureType = t.TextureType;
                            et.FFilePath = t.FilePath;
                            et.FTiling = t.Tiling;
                        }
                        else
                        {
                            var nt = new TColorOptionTexture
                            {
                                FColorOptionId = targetOpt.FColorOptionId,
                                FTextureType = t.TextureType,
                                FFilePath = t.FilePath,
                                FTiling = t.Tiling
                            };
                            _db.TColorOptionTextures.Add(nt);
                        }
                    }
                    _db.SaveChanges();
                }
            }

            _db.SaveChanges();
            tx.Commit();
            return product.FProductId;
        }


    }

}
