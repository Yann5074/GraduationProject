namespace ApiProject.DTOs
{
    public class ResColorOptionDTO
    {
        public int FColorOptionId { get; set; }
        public string? FOptionName { get; set; }    // "深灰色布料"
        public string? FColorHex { get; set; }      // "#4A4A4A"
        public string? FThumbnail { get; set; }
        public decimal FPriceAdjustment { get; set; }
        public int? FDisplayOrder { get; set; }
        public bool FIsDefault { get; set; }
        public List<ResTextureDTO> Textures { get; set; }
    }
}
