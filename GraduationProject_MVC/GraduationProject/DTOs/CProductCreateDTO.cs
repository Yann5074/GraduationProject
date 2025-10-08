using GraduationProject.Models;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class CProductCreateDTO
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? PStatus { get; set; }
        public int? Discount { get; set; }

        public List<TProductVariant> Variants { get; set; } = new List<TProductVariant>();

        
        public List<TProductAsset> Assets { get; set; } = new List<TProductAsset>();
    }

}
