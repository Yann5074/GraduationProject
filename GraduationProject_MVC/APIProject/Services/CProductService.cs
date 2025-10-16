using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

namespace ApiProject.Services
{
    public class CProductService : IProductService
    {
        private readonly dbFurniMartContext _db;
        public CProductService(dbFurniMartContext db)
        {
            _db = db;
        }

        // Services/CProductService.cs

        //列出產品
        public async Task<ResultPagedDTO<ResProductListDTO>> GetAllProductsAsync(ReqProductFilterDTO filter)
        {
            var query = _db.TProducts
                .Include(p => p.Category)
                .Include(p => p.PStatus)
                .Include(p => p.ProductAssets)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color)
                .AsQueryable();

            // 篩選
            query = ApplyFilters(query, filter);

            // 排序
            query = ApplySorting(query, filter.SortBy);

            // 總筆數
            var totalCount = await query.CountAsync();

            // 分頁
            var products = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new ResProductListDTO
                {
                    FProductId = p.FProductId,
                    FName = p.FName,
                    FDescription = p.FDescription,
                    FCategoryId = p.FCategoryId,
                    CategoryName = p.Category.FName,
                    FPstatus = p.FPstatus,
                    StatusName = p.PStatus.FPStatusName,
                    FWarrantyMonth = p.FWarrantyMonth,
                    FAssemblyRequired = p.FAssemblyRequired,
                    FDiscount = p.FDiscount,
                    MainImageUrl = p.ProductAssets
                        .Where(a => a.FIsPrimary == true)
                        .OrderBy(a => a.FSortOrder)
                        .Select(a => a.FUrl)
                        .FirstOrDefault() ?? "/images/default.png",
                    TotalStock = p.ProductVariants.Sum(v => v.FStock ?? 0),
                    IsAvailable = p.FPstatus == 1 && p.ProductVariants.Sum(v => v.FStock ?? 0) > 0,
                    MinPrice = p.ProductVariants.Where(v => v.FPrice.HasValue).Min(v => v.FPrice),
                    MaxPrice = p.ProductVariants.Where(v => v.FPrice.HasValue).Max(v => v.FPrice),
                    FCreateTime = p.FCreateTime,
                    FUpdateTime = p.FUpdateTime
                })
                .ToListAsync();

            return new ResultPagedDTO<ResProductListDTO>
            {
                Data = products,
                Pagination = new ResPaginationDTO
                {
                    TotalCount = totalCount,
                    PageSize = filter.PageSize,
                    CurrentPage = filter.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                }
            };
        }


        //搜尋產品
        public async Task<List<ResProductDTO>> GetProductByProdNameAsync(string? keyword, CancellationToken ct = default)
        {
            var q = _db.TProducts
        .AsNoTracking()
        .Include(p => p.Category)
        .Include(p => p.ProductVariants)      // + Color 如需：.ThenInclude(v => v.Color)
        .Include(p => p.ProductAssets)
        .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                q = q.Where(p => (p.FName ?? "").Contains(kw));  // 以產品名稱搜尋
            }

            var result = await q
                .OrderByDescending(p => p.ProductId)
                .Select(p => new ResProductDTO
                {
                    ProductId = p.FProductId,
                    Name = p.FName,
                    CategoryId = p.FCategoryId,
                    CategoryName = p.Category != null ? p.Category.FName : null,
                    MinPrice = p.ProductVariants.Min(v => v.FPrice),
                    MaxPrice = p.ProductVariants.Max(v => v.FPrice),
                    PrimaryImageUrl = p.ProductAssets
                                        .OrderByDescending(a => a.FIsPrimary)
                                        .ThenBy(a => a.FSortOrder)
                                        .Select(a => a.FUrl)
                                        .FirstOrDefault(),
                    Variants = p.ProductVariants
                                .OrderBy(v => v.FPrice ?? decimal.MaxValue)
                                .Select(v => new ResProductVariantDTO
                                {
                                    FProductVariantId = v.FProductVariantId,
                                    FSku = v.FSku,
                                    FPrice = v.FPrice,
                                    FStock = v.FStock,
                                    // ColorId   = v.ColorId,
                                    // ColorName = v.Color?.ColorName,
                                    // ColorCode = v.Color?.ColorCode,
                                    FSizeLabel = v.FSizeLabel
                                }).ToList(),
                    Assets = p.ProductAssets
                                .OrderByDescending(a => a.FIsPrimary)
                                .ThenBy(a => a.FSortOrder)
                                .Select(a => new ResProductAssetDTO
                                {
                                    FAssetId = a.FAssetId,
                                    FUrl = a.FUrl,
                                    FIsPrimary = a.FIsPrimary,
                                    FSortOrder = a.FSortOrder,
                                    FMimeType = a.FMimeType
                                }).ToList()
                })
                .ToListAsync(ct);

            return result;

        }

        public async Task<ResFilterOptionsDTO> GetFilterOptionsAsync()
        {
            // 分類
            var categories = await _db.TCategories
                .Include(c => c.TProducts)
                .Where(c => c.FIsActive == true)
                .Select(c => new ResCategoryOptionDTO
                {
                    CategoryId = c.FCategoryId,
                    Name = c.FName,
                    ProductCount = c.TProducts.Count(p => p.FPstatus == 1)
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            // 價格範圍
            var priceRange = await _db.TProductVariants
                .Where(v => v.FPrice.HasValue && v.FPstatus == 1)
                .GroupBy(v => 1)
                .Select(g => new ResPriceRangeDTO
                {
                    MinPrice = g.Min(v => v.FPrice.Value),
                    MaxPrice = g.Max(v => v.FPrice.Value)
                })
                .FirstOrDefaultAsync() ?? new ResPriceRangeDTO { MinPrice = 0, MaxPrice = 0 };

            // 狀態
            var statusOptions = await _db.TPstatuses
                .OrderBy(s => s.FPStatus)
                .Select(s => new ResStatusOptionDTO
                {
                    FPStatus = s.FPStatus,
                    FPStatusName = s.FPStatusName
                })
                .ToListAsync();

            return new ResFilterOptionsDTO
            {
                Categories = categories,
                PriceRange = priceRange,
                StatusOptions = statusOptions
            };
        }


        public async Task<ResProductDetailDTO> GetProductByIdAsync(int id)
        {
            var product = await _db.TProducts
                .Include(p => p.Category)
                .Include(p => p.PStatus)
                .Include(p => p.ProductAssets)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color)
                .Where(p => p.FProductId == id)
                .Select(p => new ResProductDetailDTO
                {
                    FProductId = p.FProductId,
                    FName = p.FName,
                    FDescription = p.FDescription,
                    FCategoryId = p.FCategoryId,
                    CategoryName = p.Category.FName,
                    FPstatus = p.FPstatus,
                    StatusName = p.PStatus.FPStatusName,
                    FWarrantyMonth = p.FWarrantyMonth,
                    FAssemblyRequired = p.FAssemblyRequired,
                    FAssemblyPart = p.FAssemblyPart,
                    FDiscount = p.FDiscount,
                    MainImageUrl = p.ProductAssets
                        .Where(a => a.FIsPrimary == true)
                        .OrderBy(a => a.FSortOrder)
                        .Select(a => a.FUrl)
                        .FirstOrDefault() ?? "/images/default.png",
                    TotalStock = p.ProductVariants.Sum(v => v.FStock ?? 0),
                    IsAvailable = p.FPstatus == 1,
                    MinPrice = p.ProductVariants.Where(v => v.FPrice.HasValue).Min(v => v.FPrice),
                    MaxPrice = p.ProductVariants.Where(v => v.FPrice.HasValue).Max(v => v.FPrice),
                    FCreateTime = p.FCreateTime,
                    FUpdateTime = p.FUpdateTime,

                    Variants = p.ProductVariants.Select(v => new ResProductVariantDTO
                    {
                        FProductVariantId = v.FProductVariantId,
                        FProductId = v.FProductId,
                        FSku = v.FSku,
                        FPrice = v.FPrice,
                        FCost = v.FCost,
                        FStock = v.FStock,
                        FPstatus = v.FPstatus,
                        FColorId = v.FColorId,
                        ColorName = v.Color.FColorName,
                        ColorCode = v.Color.FColorCode,
                        FLength = v.FLength,
                        FWidth = v.FWidth,
                        FHeight = v.FHeight,
                        FSizeLabel = v.FSizeLabel,
                        FWeight = v.FWeight
                    }).ToList(),

                    Assets = p.ProductAssets
                        .OrderBy(a => a.FSortOrder)
                        .Select(a => new ResProductAssetDTO
                        {
                            FAssetId = a.FAssetId,
                            FProductId = a.FProductId,
                            FProductVariantId = a.FProductVariantId,
                            FPicture = a.FPicture,
                            FAssetType = a.FAssetType,
                            FUrl = a.FUrl,
                            FIsPrimary = a.FIsPrimary,
                            FSortOrder = a.FSortOrder
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            return product;
        }



        /*
         ///////////////////////////////////////////////
         */
        private IQueryable<TProduct> ApplyFilters(IQueryable<TProduct> query, ReqProductFilterDTO filter)
        {
            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.FCategoryId == filter.CategoryId.Value);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.ProductVariants.Any(v => v.FPrice >= filter.MinPrice.Value));

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.ProductVariants.Any(v => v.FPrice <= filter.MaxPrice.Value));

            if (filter.StatusId.HasValue)
                query = query.Where(p => p.FPstatus == filter.StatusId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();
                query = query.Where(p =>
                    p.FName.ToLower().Contains(keyword) ||
                    (p.FDescription != null && p.FDescription.ToLower().Contains(keyword)));
            }

            return query;
        }

        private IQueryable<TProduct> ApplySorting(IQueryable<TProduct> query, string sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.ProductVariants.Where(v => v.FPrice.HasValue).Min(v => v.FPrice)),
                "price_desc" => query.OrderByDescending(p => p.ProductVariants.Where(v => v.FPrice.HasValue).Max(v => v.FPrice)),
                "name" => query.OrderBy(p => p.FName),
                "created_asc" => query.OrderBy(p => p.FCreateTime),
                "created_desc" => query.OrderByDescending(p => p.FCreateTime),
                _ => query.OrderByDescending(p => p.FCreateTime)
            };
        }

    }


}


