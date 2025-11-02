namespace ApiProject.DTOs
{
    public class ResVariantDTO
    {
        public int FProductVariantId { get; set; }
        public string? FSku { get; set; }
        public decimal? FPrice { get; set; }
        public int? FStock { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHex { get; set; }
    }
}
