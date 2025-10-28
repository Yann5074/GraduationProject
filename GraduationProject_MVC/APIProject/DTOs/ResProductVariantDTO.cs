namespace ApiProject.DTOs
{
    public class ResProductVariantDTO
    {
        public int FProductVariantId { get; set; }
        public int? FProductId { get; set; }
        public string FSku { get; set; }
        public decimal? FPrice { get; set; }
        public decimal? FCost { get; set; }
        public int? FStock { get; set; }
        public int? FPstatus { get; set; }
        public int? FColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorCode { get; set; }
        public string? ColorHex { get; set; }
        public decimal? FLength { get; set; }
        public decimal? FWidth { get; set; }
        public decimal? FHeight { get; set; }
        public string? FSizeLabel { get; set; }
        public decimal? FWeight { get; set; }

      


    }
}
