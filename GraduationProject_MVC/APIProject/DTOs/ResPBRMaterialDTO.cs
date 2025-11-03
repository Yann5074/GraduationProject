namespace ApiProject.DTOs
{
    public class ResPBRMaterialDTO
    {
        public int ProductId { get; set; }
        public int? ColorId { get; set; }
        public int? ProductVariantId { get; set; }
        public string ModelUrl { get; set; } = "";
        public string BaseColorUrl { get; set; } = "";
        public string? MetallicUrl { get; set; }
        public string? RoughnessUrl { get; set; }
        public string? MetallicRoughnessUrl { get; set; }
        public string? NormalUrl { get; set; }
        public string? AoUrl { get; set; }
        public string? EmissiveUrl { get; set; }
        public decimal ModelScale { get; set; } = 1.0m;
    }
}
