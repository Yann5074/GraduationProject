namespace GraduationProject.DTOs
{
    public class CProductVariantUpdateDTO
    {
        public int? ProductVariantId { get; set; }   // null = 新增
        public bool? Deleted { get; set; }           // true = 刪除
        public string? SKU { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public int? Stock { get; set; }
        public int? PStatus { get; set; }
        public int? ColorId { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public string? SizeLabel { get; set; }
        public decimal? Weight { get; set; }
    }
}
