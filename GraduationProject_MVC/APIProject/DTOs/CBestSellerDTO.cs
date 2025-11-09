namespace ApiProject.DTOs
{
    public sealed record class CBestSellerDTO
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int TotalQuantity { get; init; }
        public decimal TotalSalesAmount { get; init; }
        public int VariantCount { get; init; }
        public int? TopVariantId { get; init; }
        public string? TopVariantSKU { get; init; }
    }
}


