using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class CProductCreateDto
    {
         public string? Name { get; set; }
         public int? CategoryId { get; set; }


        [DisplayName("保固期")]
        public int? WarrantyMonth { get; set; }

        [DisplayName("描述")]
        public string? Description { get; set; }
        public bool? AssemblyRequired { get; set; }
        public string? AssemblyPart { get; set; }
        public int? Discount { get; set; }
        [Required] public int? PStatus { get; set; }

        // children
        [MinLength(1, ErrorMessage = "至少需要建立一個規格/型號。")]
        public List<CProductVariantDto> Variants { get; set; } = new();

        public List<CProductAssetDto> Assets { get; set; } = new();
    }
}
