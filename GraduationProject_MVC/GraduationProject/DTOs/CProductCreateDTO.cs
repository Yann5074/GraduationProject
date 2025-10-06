using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class CProductCreateDTO
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = default!;

        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int? PStatus { get; set; }
        public int? Discount { get; set; }
    }
}
