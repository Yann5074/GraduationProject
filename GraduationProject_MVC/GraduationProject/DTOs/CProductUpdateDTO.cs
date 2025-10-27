using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
  
    public class CProductUpdateDTO : CProductCreateDTO
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
        public new List<CProductAssetDTO> Assets { get; set; } = new();
        public List<CProductVariantDTO> Variants { get; set; } = new();
    
    }
}
