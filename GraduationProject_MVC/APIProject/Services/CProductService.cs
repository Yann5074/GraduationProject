using Microsoft.EntityFrameworkCore;
using ApiProject.Models;

public interface IProductService
{
    Task<List<ProductPickDto>> SearchAsync(string? categoryKeyword, string? colorKeyword, decimal? min, decimal? max);
}

public class CProductService : IProductService
{
    private readonly dbFurniMartContext _ctx;
    public CProductService(dbFurniMartContext ctx) => _ctx = ctx;

    public async Task<List<ProductPickDto>> SearchAsync(string? categoryKeyword, string? colorKeyword, decimal? min, decimal? max)
    {
        // 三表 join（主軸：tProductVariant）
        var query =
            from v in _ctx.tProductVariant.AsNoTracking()
            join cat in _ctx.tCategory.AsNoTracking() on v.fCategoryId equals cat.fCategoryId
            join col in _ctx.tColor.AsNoTracking() on v.fColorId equals col.fColorId
            select new
            {
                VariantId = v.fProductVariantId,     // 若你要回傳 Variant 的主鍵
                ProductId = v.fProductId,            // 若表上有 ProductId
                Name = v.fName,                 // 若變體表就有顯示名稱；沒有可換其他欄位
                Category = cat.fName,
                Color = col.fColorName,
                Price = v.fPrice
            };

        if (!string.IsNullOrWhiteSpace(categoryKeyword))
            query = query.Where(x => x.Category.Contains(categoryKeyword));

        if (!string.IsNullOrWhiteSpace(colorKeyword))
            query = query.Where(x => x.Color.Contains(colorKeyword));

        if (min.HasValue) query = query.Where(x => x.Price >= min.Value);
        if (max.HasValue) query = query.Where(x => x.Price <= max.Value);

        return await query
            .OrderBy(x => x.Price)
            .Take(5)
            .Select(x => new ProductPickDto
            {
                // 若你只需要其中一個 Id，擇一回即可
                ProductId = x.ProductId,   // 如果沒有 ProductId，可改成 VariantId
                Name = x.Name ?? $"{x.Color}{x.Category}", // 沒有名稱時給個 fallback
                Category = x.Category,
                Color = x.Color,
                Price = x.Price
            })
            .ToListAsync();
    }
}

// 輕量回傳用 DTO（不是資料表）
public class ProductPickDto
{
    public int ProductId { get; set; }       // 若你想回 VariantId，可把型別/名稱改掉
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Color { get; set; } = "";
    public decimal Price { get; set; }
}
