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
        public string? PStatus { get; set; }
        [Display(Name = "顏色")]
        public string? ColorName { get; set; }

        
        public string? FPStatusName { get; set; }

        [Display(Name = "價格")]
        public decimal? Price { get; set; }

        [Display(Name = "成本")]
        public decimal? Cost { get; set; }
        public int? Discount { get; set; }

        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }

    }
}
