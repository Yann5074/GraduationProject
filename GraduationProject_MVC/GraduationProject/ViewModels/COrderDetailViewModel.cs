using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace GraduationProject.ViewModels
{
    public class COrderDetailViewModel
    {
       
        public int OrderId { get; set; }
        [Required]
        public int ProductVariantId { get; set; }
        [Display(Name ="商品名稱")]
        public string ProductName { get; set; }
        public int IsDeleted { get; set; }
        [Required]
        [Display(Name ="商品數量")]
        public int Quantity { get; set; }
        [Display(Name ="商品單價")]
        public decimal UnitPrice { get; set; }

    }
}