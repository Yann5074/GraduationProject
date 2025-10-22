using GraduationProject.Models;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class CProductVariantDTO
    {
        public int VariantId { get; set; }
        public string? SKU { get; set; }
        public int? ColorId { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? SizeLabel { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public int? Stock { get; set; }

        public string? PStatusName { get; set; }  // 顯示用

        public string? ColorName { get; set; }
        public string? ColorCode { get; set; }
        public int? PStatus { get; set; }        // tProductVariant.fPStatus

    
    }

}
