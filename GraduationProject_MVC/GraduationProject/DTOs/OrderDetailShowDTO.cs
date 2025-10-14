using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class OrderDetailShowDTO
    {
        public int OrderId { get; set; }
        
        public int ProductVariantId { get; set; }
        [Display(Name ="商品名稱")]
        public string ProductName { get; set; }
        public int IsDeleted { get; set; }
        [Display(Name ="單價")]
        public decimal UnitPrice { get; set; }
        [Display(Name ="數量")]
        public int Quantity { get; set; }
    }
}