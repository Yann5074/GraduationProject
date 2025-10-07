using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class COrderDetailViewModel
    {
        [Required]
        public int ProductVariantId { get; set; }
        [Required]
        public int Quantity { get; set; }

        public int UnitPrice { get; set; }
    }
}