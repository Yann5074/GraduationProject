using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class CProductVariantDto
    {
         public string? SKU { get; set; }
        [Range(0, 9999999)] public decimal? Price { get; set; }
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
