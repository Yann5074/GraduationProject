using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class CProductColorOptionDTO
    {
        public string OptionName { get; set; } = default!;
        public string? ColorHex { get; set; }
        public string? Thumbnail { get; set; }
        public decimal? PriceAdjustment { get; set; }
        public bool? IsDefault { get; set; }
        public int? DisplayOrder { get; set; }
        public List<CTextureEditDTO> Textures { get; set; } = new();
    }
}
