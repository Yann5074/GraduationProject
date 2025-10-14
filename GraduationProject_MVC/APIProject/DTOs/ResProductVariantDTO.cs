namespace ApiProject.DTOs
{
    public class ResProductVariantDTO
    {
        public int ProductVariantId { get; init; }
        public string? SKU { get; init; }     // 來自 tProductVariant.fSKU
        public decimal? Price { get; init; }  // tProductVariant.fPrice
        public int? Stock { get; init; }
        public int? PStatus { get; init; }
        public int? ColorId { get; init; }
        public string? ColorName { get; init; } // join tColor
        public string? ColorCode { get; init; }
        public string? SizeLabel { get; init; } // fSizeLabel
    }
}
