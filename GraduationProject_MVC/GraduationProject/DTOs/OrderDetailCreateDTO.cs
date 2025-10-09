using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class OrderDetailCreateDTO
    {
        public int OrderId { get; set; }

        public int ProductVariantId { get; set; }

        public int IsDeleted { get; set; }

        public int UnitPrice { get; set; }

        public int Quantity { get; set; }
    }
}