using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.Text;

namespace ApiProject.Services
{
    public class CProductService : IProductService
    {
        private readonly dbFurniMartContext _db;

        // SKU 格式常數
        private const int PRODUCT_CODE_LENGTH = 4;  // 產品代碼長度
        private const int PART_CODE_LENGTH = 3;     // 部位代碼長度
        private const int OPTION_ID_LENGTH = 2;     // 選項 ID 長度
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
                        .FirstOrDefault() ?? "/ProductImages/default.png",
                    TotalStock = p.ProductVariants.Sum(v => v.FStock ?? 0),
                    IsAvailable = p.FPstatus == 1 && p.ProductVariants.Sum(v => v.FStock ?? 0) > 0,
                    MinPrice = p.ProductVariants.Where(v => v.FPrice.HasValue).Min(v => v.FPrice),
                    MaxPrice = p.ProductVariants.Where(v => v.FPrice.HasValue).Max(v => v.FPrice)
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
        .Include(p => p.ProductVariants)
            .ThenInclude(v => v.Color)
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
                                    FColorId = v.FColorId,
                                    ColorName = v.Color.FColorName,
                                    ColorCode = v.Color.FColorCode,
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


        public async Task<ResProductDetailDTO?> GetProductByIdAsync(int id)
        {
            // 基本查詢 - 只使用 7 個資料表
            var product = await _db.TProducts
                .Include(p => p.Category)
                .Include(p => p.ProductAssets)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color)
                .FirstOrDefaultAsync(p => p.FProductId == id);

            if (product == null)
            {
                return null;
            }

            // 取得主圖
            var primaryAsset = product.ProductAssets
                .Where(a => !string.IsNullOrEmpty(a.FUrl) && (a.FIsPrimary ?? false))
                .OrderBy(a => a.FSortOrder)
                .FirstOrDefault();

            // 取得 3D 模型
            var modelAsset = product.ProductAssets
                .FirstOrDefault(a => a.FAssetType == "3D" || a.FAssetType == "Model");

            // 取得環境貼圖
            var envMapAsset = product.ProductAssets
                .FirstOrDefault(a => a.FAssetType == "EnvMap");

            var result = new ResProductDetailDTO
            {
                FProductId = product.FProductId,
                FName = product.FName,
                FDescription = product.FDescription,
                FCategoryId = product.FCategoryId,
                CategoryName = product.Category?.FName,
                FWarrantyMonth = product.FWarrantyMonth,
                FAssemblyRequired = product.FAssemblyRequired,


                // 主圖 URL
                MainImageUrl = primaryAsset?.FUrl
                    ?? product.ProductAssets
                        .Where(a => !string.IsNullOrEmpty(a.FUrl))
                        .OrderBy(a => a.FSortOrder)
                        .Select(a => a.FUrl)
                        .FirstOrDefault()
                    ?? "/ProductImages/default.png",

                // 3D 模型路徑
                F3dModelPath = modelAsset?.FUrl,

                // 環境貼圖路徑
                EnvMapUrl = envMapAsset?.FUrl,

                // 素材列表
                Assets = product.ProductAssets
                    .Where(a => !string.IsNullOrEmpty(a.FUrl))
                    .OrderBy(a => a.FSortOrder)
                    .Select(a => new ResProductAssetDTO
                    {
                        FAssetId = a.FAssetId,
                        FAssetType = a.FAssetType,
                        FFilePath = a.FUrl,
                        FUrl = a.FUrl,
                        FMimeType = a.FMimeType,
                        FPosterUrl = a.FPosterUrl,
                        FIsPrimary = a.FIsPrimary ?? false,
                        FSortOrder = a.FSortOrder ?? 0
                    }).ToList(),

                // 變體列表（只顯示上架中的）
                Variants = product.ProductVariants
                    .Where(v => v.FPstatus == 1)
                    .Select(v => new ResProductVariantDTO
                    {
                        FProductVariantId = v.FProductVariantId,
                        FSku = v.FSku,
                        FPrice = v.FPrice,
                        FStock = v.FStock,
                        FColorId = v.FColorId,
                        ColorName = v.Color?.FColorName,
                        ColorCode = v.Color?.FColorCode,
                        ColorHex = v.Color?.FColorHex ?? v.Color?.FColorCode,
                        FSizeLabel = v.FSizeLabel
                    }).ToList(),

                // 庫存總計（只計算上架中的變體）
                TotalStock = product.ProductVariants
                    .Where(v => v.FPstatus == 1)
                    .Sum(v => v.FStock ?? 0),

                // 是否可購買
                IsAvailable = product.FPstatus == 1 &&
                              product.ProductVariants
                                  .Where(v => v.FPstatus == 1)
                                  .Sum(v => v.FStock ?? 0) > 0,

                // 價格區間（只計算上架中的變體）
                MinPrice = product.ProductVariants
                    .Where(v => v.FPrice.HasValue && v.FPstatus == 1)
                    .Min(v => v.FPrice),

                MaxPrice = product.ProductVariants
                    .Where(v => v.FPrice.HasValue && v.FPstatus == 1)
                    .Max(v => v.FPrice),


                // 時間戳記
                FCreateTime = product.FCreateTime,
                FUpdateTime = product.FUpdateTime
            };

            return result;
        }


        public async Task<List<ResProductListDTO>> GetSimilarProductsAsync(int productId, int count = 4)
        {
            // 1. 先取得當前商品的資訊
            var currentProduct = await _db.TProducts
                .Include(p => p.ProductVariants)
                .Where(p => p.FProductId == productId)
                .Select(p => new
                {
                    p.FProductId,
                    p.FCategoryId,
                    // 計算該商品的平均價格
                    AvgPrice = p.ProductVariants
                        .Where(v => v.FPrice.HasValue)
                        .Average(v => v.FPrice)
                })
                .FirstOrDefaultAsync();

            // 如果找不到商品，回傳空列表
            if (currentProduct == null)
                return new List<ResProductListDTO>();

            // 2. 計算價格範圍（±30%）
            decimal? minPrice = null;
            decimal? maxPrice = null;

            if (currentProduct.AvgPrice.HasValue)
            {
                minPrice = currentProduct.AvgPrice.Value * 0.7m;  // -30%
                maxPrice = currentProduct.AvgPrice.Value * 1.3m;  // +30%
            }

            // 3. 查詢相似商品
            var query = _db.TProducts
                .Include(p => p.Category)
                .Include(p => p.PStatus)
                .Include(p => p.ProductAssets)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color)
                .Where(p =>
                    p.FProductId != productId &&           // 排除當前商品
                    p.FPstatus == 1 &&                     // 只要上架的商品
                    p.FCategoryId == currentProduct.FCategoryId  // 相同分類
                )
                .AsQueryable();

            // 4. 如果有價格範圍，加入價格篩選
            if (minPrice.HasValue && maxPrice.HasValue)
            {
                query = query.Where(p => p.ProductVariants
                    .Any(v => v.FPrice >= minPrice && v.FPrice <= maxPrice));
            }

            // 5. 取得指定數量的商品
            var similarProducts = await query
                .Take(count)
                .Select(p => new ResProductListDTO
                {
                    FProductId = p.FProductId,
                    FName = p.FName,
                    FDescription = p.FDescription,
                    FCategoryId = p.FCategoryId,
                    CategoryName = p.Category.FName,
                    FPstatus = p.FPstatus,
                    StatusName = p.PStatus.FPstatusName,
                    FWarrantyMonth = p.FWarrantyMonth,
                    FAssemblyRequired = p.FAssemblyRequired,
                    FDiscount = p.FDiscount,
                    MainImageUrl = p.ProductAssets
                        .Where(a => a.FIsPrimary == true)
                        .OrderBy(a => a.FSortOrder)
                        .Select(a => a.FUrl)
                        .FirstOrDefault() ?? "/ProductImages/default.png",
                    TotalStock = p.ProductVariants.Sum(v => v.FStock ?? 0),
                    IsAvailable = p.FPstatus == 1 && p.ProductVariants.Sum(v => v.FStock ?? 0) > 0,
                    MinPrice = p.ProductVariants.Where(v => v.FPrice.HasValue).Min(v => v.FPrice),
                    MaxPrice = p.ProductVariants.Where(v => v.FPrice.HasValue).Max(v => v.FPrice),
                    FCreateTime = p.FCreateTime,
                    FUpdateTime = p.FUpdateTime
                })
                .ToListAsync();

            return similarProducts;
        }

        // 批次取得購物車商品資訊
        public async Task<List<ResCartProductDTO>> GetCartProductsAsync(List<int> productVariantIds)
        {
 
            if (productVariantIds == null || !productVariantIds.Any())
            {
                return new List<ResCartProductDTO>();
            }


            var variants = await _db.TProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.ProductAssets)
                .Include(v => v.Product)
                    .ThenInclude(p => p.ProductParts)
                .Where(v => productVariantIds.Contains(v.FProductVariantId))
                .ToListAsync();

            // 組裝 DTO
            var result = new List<ResCartProductDTO>();

            foreach (var variant in variants)
            {
                var cartProduct = await BuildCartProductDTO(variant);
                result.Add(cartProduct);
            }

            return result;
        }







        private async Task<ResCartProductDTO> BuildCartProductDTO(TProductVariant variant)
        {
            // 解析 SKU 取得顏色組合
            var CartcolorOptions = await ParseSKUForCart(variant.Product.FProductId, variant.FSku);

            // 建立顏色組合描述文字
            var colorDescription = CartcolorOptions.Any()
                ? string.Join(" + ", CartcolorOptions.Select(c => c.OptionName))
                : null;

            return new ResCartProductDTO
            {
                FProductVariantId = variant.FProductVariantId,
                FProductId = variant.Product.FProductId,
                FProductName = variant.Product.FName,
                FSKU = variant.FSku,
                FPrice = variant.FPrice ?? 0,
                FStock = variant.FStock ?? 0,
                IsAvailable = (variant.FStock ?? 0) > 0 && variant.FPstatus == 1 && variant.Product.FPstatus == 1,
                FProductStatus = variant.Product.FPstatus,
                FVariantStatus = variant.FPstatus,
                MainImageUrl = variant.Product.ProductAssets
                    .Where(a => a.FIsPrimary == true)
                    .OrderBy(a => a.FSortOrder)
                    .Select(a => a.FUrl)
                    .FirstOrDefault() ?? "/ProductImages/default.png",
                IsCustomizable = variant.Product.ProductParts.Any(),
                ColorCombinationDescription = colorDescription,
                CartColorOptions = CartcolorOptions
            };
        }


        // 解析 SKU 取得購物車用的顏色資訊
        // SKU（例如：EAGOTOP02LEG05）
        // 顏色選項列表
        private async Task<List<ResCartColorOptionDTO>> ParseSKUForCart(int productId, string sku)
        {
            // 驗證輸入
            if (string.IsNullOrEmpty(sku))
            {
                return new List<ResCartColorOptionDTO>();
            }

            // 移除產品代碼部分
            if (sku.Length <= PRODUCT_CODE_LENGTH)
            {
                return new List<ResCartColorOptionDTO>();
            }

            var optionsPart = sku.Substring(PRODUCT_CODE_LENGTH);

            // 取得產品的部位順序
            var parts = await _db.TProductParts
                .Include(p => p.ColorOptions)
                .Where(p => p.FProductId == productId)
                .OrderBy(p => p.FDiaplayOrder)
                .ToListAsync();

            var result = new List<ResCartColorOptionDTO>();
            int position = 0;

            // 逐一解析每個部位
            foreach (var part in parts)
            {
                // 檢查是否還有足夠的字元
                if (position + PART_CODE_LENGTH + OPTION_ID_LENGTH > optionsPart.Length)
                {
                    break;
                }

                // 讀取部位代碼（3 字元）
                string partCode = optionsPart.Substring(position, PART_CODE_LENGTH);
                position += PART_CODE_LENGTH;

                // 讀取選項 ID（2 字元）
                string optionIdStr = optionsPart.Substring(position, OPTION_ID_LENGTH);
                position += OPTION_ID_LENGTH;

                // 驗證部位代碼並轉換選項 ID
                if (partCode == part.FPartCode && int.TryParse(optionIdStr, out int optionId))
                {
                    // 查找對應的顏色選項
                    var colorOption = part.ColorOptions.FirstOrDefault(o => o.FColorOptionId == optionId);

                    if (colorOption != null)
                    {
                        result.Add(new ResCartColorOptionDTO
                        {
                            PartName = part.FPartName,
                            OptionName = colorOption.FOptionName,
                            ColorHex = colorOption.FColorHex,
                            Thumbnail = colorOption.FThumbnail
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 根據使用者選擇的顏色組合查詢價格和庫存
        /// </summary>
        public async Task<ResProductPriceDTO> GetPriceByCustomizationAsync(
            int productId,
            Dictionary<string, int> selectedOptions)
        {
            // 驗證輸入
            if (selectedOptions == null || !selectedOptions.Any())
            {
                return null;
            }

            // 建立 SKU
            var sku = await BuildSKUAsync(productId, selectedOptions);

            if (string.IsNullOrEmpty(sku))
            {
                return null;
            }

            // 查詢對應的產品變體
            var variant = await _db.TProductVariants
                .Where(v =>
                    v.FProductId == productId &&
                    v.FSku == sku &&
                    v.FPstatus == 1)
                .Select(v => new ResProductPriceDTO
                {
                    FProductVariantId = v.FProductVariantId,
                    FSKU = v.FSku,
                    FPrice = v.FPrice ?? 0,
                    FStock = v.FStock ?? 0,
                    IsAvailable = (v.FStock ?? 0) > 0 && v.FPstatus == 1,
                    FVariantStatus = v.FPstatus
                })
                .FirstOrDefaultAsync();

            return variant;
        }

        /// <summary>
        /// 批次檢查庫存
        /// </summary>
        public async Task<ResStockCheckDTO> CheckStockAsync(List<ReqStockCheckItemDTO> items)
        {
            var result = new ResStockCheckDTO
            {
                IsAvailable = true,
                Warnings = new List<StockWarningDTO>()
            };

            // 驗證輸入
            if (items == null || !items.Any())
            {
                return result;
            }

            // 取得所有需要檢查的產品變體 ID
            var variantIds = items.Select(i => i.ProductVariantId).Distinct().ToList();

            // 批次查詢產品變體資訊
            var variants = await _db.TProductVariants
                .Include(v => v.Product)
                .Where(v => variantIds.Contains(v.FProductVariantId))
                .ToListAsync();

            // 逐一檢查每個商品
            foreach (var item in items)
            {
                var variant = variants.FirstOrDefault(v => v.FProductVariantId == item.ProductVariantId);

                if (variant == null)
                {
                    // 商品不存在
                    result.IsAvailable = false;
                    result.Warnings.Add(new StockWarningDTO
                    {
                        ProductVariantId = item.ProductVariantId,
                        ProductName = "未知商品",
                        SKU = "",
                        RequestedQuantity = item.Quantity,
                        AvailableStock = 0,
                        Message = "商品不存在",
                        WarningType = "not_found"
                    });
                    continue;
                }

                // 檢查商品狀態
                if (variant.FPstatus != 1 || variant.Product.FPstatus != 1)
                {
                    // 商品已下架
                    result.IsAvailable = false;
                    result.Warnings.Add(new StockWarningDTO
                    {
                        ProductVariantId = item.ProductVariantId,
                        ProductName = variant.Product.FName,
                        SKU = variant.FSku,
                        RequestedQuantity = item.Quantity,
                        AvailableStock = variant.FStock ?? 0,
                        Message = $"{variant.Product.FName} 已下架",
                        WarningType = "unavailable"
                    });
                    continue;
                }

                var availableStock = variant.FStock ?? 0;

                // 檢查庫存
                if (availableStock <= 0)
                {
                    // 完全缺貨
                    result.IsAvailable = false;
                    result.Warnings.Add(new StockWarningDTO
                    {
                        ProductVariantId = item.ProductVariantId,
                        ProductName = variant.Product.FName,
                        SKU = variant.FSku,
                        RequestedQuantity = item.Quantity,
                        AvailableStock = 0,
                        Message = $"{variant.Product.FName} 目前缺貨",
                        WarningType = "out_of_stock"
                    });
                }
                else if (availableStock < item.Quantity)
                {
                    // 庫存不足
                    result.IsAvailable = false;
                    result.Warnings.Add(new StockWarningDTO
                    {
                        ProductVariantId = item.ProductVariantId,
                        ProductName = variant.Product.FName,
                        SKU = variant.FSku,
                        RequestedQuantity = item.Quantity,
                        AvailableStock = availableStock,
                        Message = $"{variant.Product.FName} 庫存不足，需要 {item.Quantity} 件，僅剩 {availableStock} 件",
                        WarningType = "insufficient_stock"
                    });
                }
                // 如果庫存充足，不加入警告
            }

            return result;
        }


        /// SKU（內部方法）
        private async Task<string> BuildSKUAsync(int productId, Dictionary<string, int> selectedOptions)
        {
            // 1. 取得產品代碼（4字元）
            var productCode = await GetProductCodeAsync(productId);
            if (string.IsNullOrEmpty(productCode) || productCode.Length != PRODUCT_CODE_LENGTH)
            {
                return null;
            }

            // 2. 取得產品的所有部位（按順序）
            var parts = await _db.TProductParts
                .Where(p => p.FProductId == productId)
                .OrderBy(p => p.FDiaplayOrder)
                .Select(p => p.FPartCode)
                .ToListAsync();

            if (!parts.Any())
            {
                return null;
            }

            // 3. 建立 SKU
            var skuBuilder = new StringBuilder(productCode);

            foreach (var partCode in parts)
            {
                // 檢查部位代碼長度
                if (partCode.Length != PART_CODE_LENGTH)
                {
                    return null;
                }

                // 檢查此部位是否有選擇
                if (selectedOptions.TryGetValue(partCode, out int optionId))
                {
                    // 部位代碼（3字元）
                    skuBuilder.Append(partCode);

                    // 選項 ID（2位數，不足補0）
                    skuBuilder.Append(optionId.ToString($"D{OPTION_ID_LENGTH}"));
                }
                else
                {
                    // 如果有部位沒選擇，無法建立完整 SKU
                    return null;
                }
            }

            return skuBuilder.ToString();
        }

        /// <summary>
        /// 取得產品代碼（固定4字元）
        /// </summary>
        private async Task<string> GetProductCodeAsync(int productId)
        {
            var productName = await _db.TProducts
                .Where(p => p.FProductId == productId)
                .Select(p => p.FName)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(productName))
            {
                return null;
            }

            // 移除空白和特殊字元，只保留英文和數字
            var cleanName = new string(productName
                .Where(c => char.IsLetterOrDigit(c))
                .ToArray())
                .ToUpper();

            // 取前4個字元，不足補X
            if (cleanName.Length >= PRODUCT_CODE_LENGTH)
            {
                return cleanName.Substring(0, PRODUCT_CODE_LENGTH);
            }
            else
            {
                return cleanName.PadRight(PRODUCT_CODE_LENGTH, 'X');
            }
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


