using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
  
    public class CProductUpdateDTO
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = default!;
        public int? CategoryId { get; set; }
        public string? Description { get; set; }
        public int? PStatus { get; set; }
        public int? WarrantyMonth { get; set; }
        public bool? AssemblyRequired { get; set; }
        public string? AssemblyPart { get; set; }
        public int? Discount { get; set; }

        public List<CProductVariantUpdateDTO> Variants { get; set; } = new();
        public List<CProductAssetUpdateDTO> Assets { get; set; } = new();
        public List<CProductPartUpdateDTO> Parts { get; set; } = new();
    }
}
