using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.Intrinsics.X86;


namespace GraduationProject.Services
{
    public class CProductService : IProductService
    {
        private readonly dbFurniMartContext _db;
        private readonly string _wwwrootPath;
        public CProductService(dbFurniMartContext db, IWebHostEnvironment env)
        {
            _db = db;
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
                    Url = a.FUrl,
                    IsPrimary = a.FIsPrimary ?? false,
                    SortOrder = a.FSortOrder,
                    AssetType = a.FAssetType
                })
                .ToList();

            return vm;
           
        }

        public bool UpdateProduct(CProductEditViewModel vm)
        {
            var p = _db.TProducts
        .Include(x => x.ProductVariants)
            .ThenInclude(v => v.PStatus)
        .Include(x => x.ProductAssets)
        .FirstOrDefault(x => x.FProductId == vm.ProductId);
            if (p == null) return false;

            // 商品本體
            p.FName = vm.Name?.Trim();
            p.FDescription = vm.Description?.Trim();
            if (vm.CategoryId.HasValue) p.FCategoryId = vm.CategoryId.Value;
            if (vm.PStatusId.HasValue) p.FPstatus = vm.PStatusId.Value;
            p.FWarrantyMonth = vm.WarrantyMonth;
            p.FAssemblyRequired = vm.AssemblyRequired;
            p.FAssemblyPart = vm.AssemblyPart?.Trim();

            // 變體（逐筆覆寫）
            foreach (var item in vm.Variants)
            {
                var v = p.ProductVariants.FirstOrDefault(x => x.FProductVariantId == item.ProductVariantId);
                if (v == null) continue;
                v.FPrice = item.Price;
                v.FCost = item.Cost;       
                v.FStock = item.Stock;      
                v.FLength = item.Length;     
                v.FWidth = item.Width;      
                v.FHeight = item.Height;     
                v.FWeight = item.Weight;
                v.FPstatus = item.PStatusId ?? v.FPstatus;
            }

            // 資產（更新主圖/排序）
            if (vm.Assets?.Any() == true)
            {
                // 若同時勾了多個 IsPrimary，只保留第一個
                var primaryIds = vm.Assets.Where(a => a.IsPrimary == true).Select(a => a.AssetId).ToList();
                int keepPrimaryId = primaryIds.FirstOrDefault();

                foreach (var a in vm.Assets)
                {
                    var entity = p.ProductAssets.FirstOrDefault(x => x.FAssetId == a.AssetId);
                    if (entity == null) continue;
                    entity.FUrl = a.Url?.Trim();
                    entity.FSortOrder = a.SortOrder;
                    entity.FIsPrimary = a.IsPrimary;  // 強制單一主圖
                                                        // 若需要：entity.FAssetType = a.AssetType ?? "image";
                }
            }

            // 處理新上傳圖片（可選）
            if (vm.NewPictures?.Any() == true && !string.IsNullOrEmpty(_wwwrootPath))
            {
                // 根據你的需求：統一放在 ProductImages 下，再依商品分子資料夾（可改成不分資料夾）
                var productFolder = Path.Combine(_wwwrootPath, "ProductImages", vm.ProductId.ToString());
                Directory.CreateDirectory(productFolder);

                // 取得現有排序最大值，新的往後排
                var nextSort = (_db.TProductAssets
                                    .Where(a => a.FProductId == vm.ProductId)
                                    .Max(a => (int?)a.FSortOrder) ?? 0) + 1;

                foreach (var file in vm.NewPictures)
                {
                    if (file?.Length > 0)
                    {
                        var safeName = Path.GetFileName(file.FileName);
                        var fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{safeName}";
                        var fullPath = Path.Combine(productFolder, fileName);

                        using (var fs = new FileStream(fullPath, FileMode.Create))
                            file.CopyTo(fs);

                        // 網站可用的相對路徑（重點在 /ProductImages/...）
                        var relativeUrl = $"/ProductImages/{vm.ProductId}/{fileName}";

                        _db.TProductAssets.Add(new TProductAsset
                        {
                            FProductId = vm.ProductId,
                            FUrl = relativeUrl,     // ← 這樣 <img src="..."> 能直接顯示
                            FAssetType = "image",
                            FIsPrimary = false,
                            FSortOrder = nextSort++
                        });
                    }
                }
            }

            _db.SaveChanges();
            return true;
        }

    }
}
