namespace ApiProject.DTOs;

public sealed record BestSellerDto(
    int ProductId,
    string ProductName,
    int TotalQuantity,
    decimal TotalSalesAmount,
    int VariantCount,
    int? TopVariantId,
    string? TopVariantSKU
);

