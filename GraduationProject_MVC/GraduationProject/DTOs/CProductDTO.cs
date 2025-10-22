using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class CProductDTO
    {
        public int? ProductId { get; set; }

        [Display(Name = "產品名稱")]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }

        [Display(Name = "種類")]
        public string? CategoryName { get; set; }
        [Display(Name = "狀態")]
     
        public string? ColorName { get; set; }
        public int? PStatus { get; set; }
        [Display(Name = "顏色")]

        public string? FPStatusName { get; set; }

        [Display(Name = "價格")]
        public decimal? Price { get; set; }

        [Display(Name = "成本")]
        public decimal? Cost { get; set; }
        public int? Discount { get; set; }

        public int? WarrantyMonth { get; set; }   
        public bool? AssemblyRequired { get; set; }
        public string? AssemblyPart { get; set; }


        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }


        // 圖片相關
        public string PrimaryImageUrl { get; set; }  // 主要圖片
        public List<string> ImageUrls { get; set; } = new List<string>();  // 所有圖片

        public List<CProductVariantDTO> Variants { get; set; } = new List<CProductVariantDTO>();
        public List<CProductAssetDTO> Assets { get; set; } = new List<CProductAssetDTO>();
        public List<CProductPartDTO> Parts { get; set; } = new List<CProductPartDTO>();
    }
}
