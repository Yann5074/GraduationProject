using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class CProductCreateDTO
    {
        public string Name { get; set; } = default!;
        public int? CategoryId { get; set; }
        public string? Description { get; set; }
        public int? PStatus { get; set; }         // 對應 tPStatus.fPStatus
        public int? WarrantyMonth { get; set; }
        public bool? AssemblyRequired { get; set; }
        public string? AssemblyPart { get; set; }
        public int? Discount { get; set; }

        public List<CProductVariantDTO> Variants { get; set; } = new();
        public List<CProductAssetDTO> Assets { get; set; } = new();
        public List<CProductPartDTO> Parts { get; set; } = new();
    }


}
