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


        public int Create(CProductCreateDTO dto)
        {
            // 驗證狀態/分類存在
            if (dto.PStatus.HasValue && !_db.TPstatuses.Any(s => s.FPstatus == dto.PStatus.Value))
                throw new ArgumentException("指定的商品狀態不存在");
            if (dto.CategoryId.HasValue && !_db.TCategories.Any(c => c.FCategoryId == dto.CategoryId.Value))
                throw new ArgumentException("指定的分類不存在");

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
            _db.SaveChanges();

            foreach (var v in dto.Variants)
            {
                if (v.PStatus.HasValue && !_db.TPstatuses.Any(s => s.FPstatus == v.PStatus.Value))
                    throw new ArgumentException("指定的變體狀態不存在");

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
            _db.SaveChanges();

            foreach (var a in dto.Assets)
            {
                _db.TProductAssets.Add(new TProductAsset
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
                });
            }
            _db.SaveChanges();

            tx.Commit();
            return product.FProductId;
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

            using var tx = _db.Database.BeginTransaction();

            // 主檔
            product.FName = dto.Name;
            product.FCategoryId = dto.CategoryId;
            product.FDescription = dto.Description;
            product.FPstatus = dto.PStatus;
            product.FWarrantyMonth = dto.WarrantyMonth;
            product.FAssemblyRequired = dto.AssemblyRequired;
            product.FAssemblyPart = dto.AssemblyPart;
            product.FDiscount = dto.Discount;
            product.FUpdateTime = now;

            // Variants: 刪除
            foreach (var ev in product.ProductVariants.ToList())
            {
                var incoming = dto.Variants.FirstOrDefault(v => v.VariantId == ev.FProductVariantId);
                if (incoming?.Deleted == true)
                    _db.TProductVariants.Remove(ev);
            }
            // Variants: 新增/更新
            foreach (var v in dto.Variants.Where(x => x.Deleted != true))
            {
                if (v.VariantId.HasValue)
                {
                    var ev = product.ProductVariants.FirstOrDefault(x => x.FProductVariantId == v.VariantId.Value);
                    if (ev != null)
                    {
                        ev.FSku = v.SKU;
                        ev.FPrice = v.Price; ev.FCost = v.Cost; ev.FStock = v.Stock;
                        ev.FPstatus = v.PStatus; ev.FColorId = v.ColorId;
                        ev.FLength = v.Length; ev.FWidth = v.Width; ev.FHeight = v.Height;
                        ev.FSizeLabel = v.SizeLabel; ev.FWeight = v.Weight;
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

            // Assets: 刪除
            foreach (var ea in product.ProductAssets.ToList())
            {
                var incoming = dto.Assets.FirstOrDefault(a => a.AssetId == ea.FAssetId);
                if (incoming?.Deleted == true)
                    _db.TProductAssets.Remove(ea);
            }
            // Assets: 新增/更新
            foreach (var a in dto.Assets.Where(x => x.Deleted != true))
            {
                if (a.AssetId.HasValue)
                {
                    var ea = product.ProductAssets.FirstOrDefault(x => x.FAssetId == a.AssetId.Value);
                    if (ea != null)
                    {
                        ea.FProductVariantId = a.ProductVariantId;
                        ea.FAssetType = a.AssetType; ea.FMimeType = a.MimeType;
                        ea.FUrl = a.Url; ea.FIsPrimary = a.IsPrimary; ea.FSortOrder = a.SortOrder;
                        ea.FPosterUrl = a.PosterUrl; ea.FMaterialId = a.MaterialId;
                        ea.FTexturedId = a.TexturedId; ea.FModelId = a.ModelId;
                        ea.FMetadateJson = a.MetadateJson;
                        ea.FUpdateTime = now;
                    }
                }
                else
                {
                   
                    var dup = product.ProductAssets.FirstOrDefault(x =>
                        x.FUrl == a.Url &&
                        ((x.FProductVariantId ?? 0) == (a.ProductVariantId ?? 0)));

                    if (dup != null)
                    {
                        dup.FAssetType = a.AssetType; dup.FMimeType = a.MimeType;
                        dup.FIsPrimary = a.IsPrimary; dup.FSortOrder = a.SortOrder;
                        dup.FPosterUrl = a.PosterUrl; dup.FMaterialId = a.MaterialId;
                        dup.FTexturedId = a.TexturedId; dup.FModelId = a.ModelId;
                        dup.FMetadateJson = a.MetadateJson;
                        dup.FUpdateTime = now;
                    }
                    else
                    {
                        _db.TProductAssets.Add(new TProductAsset
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
                        });
                    }
                }
            }

            _db.SaveChanges();

            tx.Commit();
            return product.FProductId;
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
