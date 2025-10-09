using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class COrderDetailCreateViewModel
    {
        public int OrderId { get; set; }
        [Required]
        [Display(Name ="產品編號")]
        public int ProductVariantId { get; set; }
        
        public int IsDeleted { get; set; }
        
        [Display(Name ="產品單價")]
        public int UnitPrice { get; set; }
        [Required]
        [Display(Name ="購買數量")]
        public int Quantity { get; set; }
    }
}