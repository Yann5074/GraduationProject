using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class CProductVariantDTO
    {
        public string SKU { get; set; }

        [Range(0.01, 9999999.99, ErrorMessage = "售價必須大於0")]
        [Display(Name = "售價")]
        public decimal Price { get; set; }

        [Range(0, 9999999.99, ErrorMessage = "成本不可為負")]
        [Display(Name = "成本")]
        public decimal? Cost { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "庫存不可為負")]
        [Display(Name = "庫存")]
        public int? Stock { get; set; }

        [Required(ErrorMessage = "請選擇顏色")]
        [Display(Name = "顏色")]
        public int? ColorId { get; set; }

        [Range(0, 999999.99)]
        [Display(Name = "長度(cm)")]
        public decimal? Length { get; set; }

        [Range(0, 999999.99)]
        [Display(Name = "寬度(cm)")]
        public decimal? Width { get; set; }

        [Range(0, 999999.99)]
        [Display(Name = "高度(cm)")]
        public decimal? Height { get; set; }

    
        [Display(Name = "尺寸標籤")]
        public string SizeLabel { get; set; }

        [Range(0, 9999.99)]
        [Display(Name = "重量(kg)")]
        public decimal? Weight { get; set; }

        [Display(Name = "變體狀態")]
        public int PStatusId { get; set; } = 1; // 預設上架中

        // 變體專屬圖片
        public List<IFormFile> VariantImages { get; set; } = new List<IFormFile>();
    }
}
