namespace ApiProject.DTOs
{
    public class ResProductDetailDTO
    {
        public int ProductId { get; init; }
        public string? Name { get; init; }
        public int CategoryId { get; init; }
        public string? CategoryName { get; init; }
        public string? Description { get; init; }
        public int? WarrantyMonth { get; init; }
        public int? PStatus { get; init; }

        public IReadOnlyList<ResProductVariantDTO> Variants { get; init; } = Array.Empty<ResProductVariantDTO>();
        public IReadOnlyList<ResProductAssetDTO> Assets { get; init; } = Array.Empty<ResProductAssetDTO>();
    }
}
