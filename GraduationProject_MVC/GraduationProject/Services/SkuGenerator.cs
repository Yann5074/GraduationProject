using GraduationProject.Models;

namespace GraduationProject.Services
{
    public class SkuGenerator
    {
       
        
        
            private readonly dbFurniMartContext _db;

            public SkuGenerator(dbFurniMartContext db)
            {
                _db = db;
            }

       
            /// <returns>唯一的 SKU 編號</returns>
            public string GenerateSku(int categoryId, int? colorId, decimal? length, decimal? width, decimal? height)
            {
                // 1. 取得分類代碼
                var category = _db.TCategories.Find(categoryId);
                string categoryCode = GetCategoryCode(category?.FName);

                // 2. 取得顏色代碼
                string colorCode = "NONE";
                if (colorId.HasValue)
                {
                    var color = _db.TColors.Find(colorId.Value);
                    colorCode = GetColorCode(color?.FColorName);
                }

                // 3. 組合尺寸代碼
                string sizeCode = GetSizeCode(length, width, height);

                // 4. 組合基礎 SKU
                string baseSku = $"{categoryCode}-{colorCode}-{sizeCode}";

                // 5. 確保唯一性（如果重複則加上流水號）
                return EnsureUniqueSku(baseSku);
            }

     
            /// 取得分類代碼
           
            private string GetCategoryCode(string categoryName)
            {
                if (string.IsNullOrWhiteSpace(categoryName))
                    return "PROD";

                // 分類名稱對照表
                var categoryMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // 子分類
                { "沙發", "SOFA" },
                { "茶几", "CTABLE" },
                { "床架", "BED" },
                { "衣櫃", "WARDROBE" },
                { "餐桌", "DTABLE" },
                { "餐椅", "DCHAIR" },
                { "書桌", "DESK" },
                { "書櫃", "BOOKSHELF" },
                { "電視櫃", "TVCAB" },
                { "邊几", "STABLE" },
                { "床頭櫃", "BEDSIDE" },
                { "鞋櫃", "SHOECAB" },
                { "收納櫃", "STORAGE" },
                
                // 父分類
                { "客廳家具", "LIVING" },
                { "臥室家具", "BEDROOM" },
                { "餐廳家具", "DINING" },
                { "書房家具", "STUDY" },
                { "戶外家具", "OUTDOOR" },
                { "家具", "FURN" }
            };

                // 如果有對照，使用對照值；否則取前4個字元
                if (categoryMapping.TryGetValue(categoryName, out string code))
                {
                    return code;
                }

                // 取中文前2個字或英文前4個字
                return categoryName.Length <= 4
                    ? categoryName.ToUpper()
                    : categoryName.Substring(0, 4).ToUpper();
            }

            /// <summary>
            /// 取得顏色代碼
            /// </summary>
            private string GetColorCode(string colorName)
            {
                if (string.IsNullOrWhiteSpace(colorName))
                    return "NONE";

                // 顏色名稱對照表
                var colorMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "白色", "WHITE" },
                { "黑色", "BLACK" },
                { "紅色", "RED" },
                { "藍色", "BLUE" },
                { "綠色", "GREEN" },
                { "黃色", "YELLOW" },
                { "橙色", "ORANGE" },
                { "粉紅色", "PINK" },
                { "紫色", "PURPLE" },
                { "灰色", "GRAY" },
                { "米色", "BEIGE" },
                { "棕色", "BROWN" },
                { "深棕色", "DKBROWN" },
                { "淺棕色", "LTBROWN" },
                { "橡木色", "OAK" },
                { "原木色", "WOOD" },
                { "淺綠色", "LTGREEN" },
                { "深綠色", "DKGREEN" },
                { "淺藍色", "LTBLUE" },
                { "深藍色", "DKBLUE" },
                { "銀色", "SILVER" },
                { "金色", "GOLD" },
                { "玫瑰金", "RGOLD" }
            };

                // 如果有對照，使用對照值；否則取前4個字元
                if (colorMapping.TryGetValue(colorName, out string code))
                {
                    return code;
                }

                return colorName.Length <= 4
                    ? colorName.ToUpper()
                    : colorName.Substring(0, 4).ToUpper();
            }


            /// 取得尺寸代碼

            private string GetSizeCode(decimal? length, decimal? width, decimal? height)
            {
                var parts = new List<string>();

                // 取整數部分
                if (length.HasValue && length.Value > 0)
                    parts.Add(((int)length.Value).ToString());

                if (width.HasValue && width.Value > 0)
                    parts.Add(((int)width.Value).ToString());

                if (height.HasValue && height.Value > 0)
                    parts.Add(((int)height.Value).ToString());

                // 如果都沒有尺寸，使用預設值
                return parts.Any() ? string.Join("-", parts) : "STD";
            }

            /// 確保 SKU 的唯一性
            /// 如果已存在相同 SKU，則在後面加上流水號

            private string EnsureUniqueSku(string baseSku)
            {
                string finalSku = baseSku;
                int counter = 1;

                // 持續檢查直到找到不重複的 SKU
                while (_db.TProductVariants.Any(v => v.FSku == finalSku))
                {
                    finalSku = $"{baseSku}-{counter:D2}"; // D2 表示補零到2位數
                    counter++;

                    // 安全機制：避免無限迴圈
                    if (counter > 999)
                    {
                        throw new InvalidOperationException($"無法為 SKU '{baseSku}' 生成唯一編號");
                    }
                }

                return finalSku;
            }


            /// 批次生成多個 SKU（用於匯入資料時）

            public List<string> GenerateSkuBatch(List<(int CategoryId, int? ColorId, decimal? Length, decimal? Width, decimal? Height)> variants)
            {
                var skus = new List<string>();

                foreach (var variant in variants)
                {
                    var sku = GenerateSku(
                        variant.CategoryId,
                        variant.ColorId,
                        variant.Length,
                        variant.Width,
                        variant.Height
                    );
                    skus.Add(sku);
                }

                return skus;
            }

            /// 驗證 SKU 格式是否正確

            public bool IsValidSkuFormat(string sku)
            {
                if (string.IsNullOrWhiteSpace(sku))
                    return false;

                // SKU 基本格式：至少要有兩個部分用 - 分隔
                var parts = sku.Split('-');
                return parts.Length >= 2;
            }


            /// 從 SKU 解析資訊（用於除錯或顯示）

            public (string Category, string Color, string Size) ParseSku(string sku)
            {
                if (string.IsNullOrWhiteSpace(sku))
                    return ("未知", "未知", "未知");

                var parts = sku.Split('-');

                if (parts.Length < 3)
                    return ("未知", "未知", "未知");

                string category = parts[0];
                string color = parts[1];
                string size = string.Join("-", parts.Skip(2));

                return (category, color, size);
            }
        
    }
}
