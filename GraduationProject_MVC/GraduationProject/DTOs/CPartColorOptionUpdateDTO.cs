namespace GraduationProject.DTOs
{
    public class CPartColorOptionUpdateDTO
    {
        public int? ColorOptionId { get; set; } // null = 新增
        public bool? Deleted { get; set; }
        public string OptionName { get; set; } = default!;
        public string? ColorHex { get; set; }
        public string? Thumbnail { get; set; }
        public decimal? PriceAdjustment { get; set; }
        public bool? IsDefault { get; set; }
        public int? DisplayOrder { get; set; }
        public List<CColorOptionTextureUpdateDTO> Textures { get; set; } = new();
    }
}
