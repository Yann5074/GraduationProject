using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class CProductPartDTO
    {
        public string PartName { get; set; } = default!;
        public string? PartCode { get; set; }
        public int? DisplayOrder { get; set; }
        public List<CProductColorOptionDTO> Options { get; set; } = new();
    }
}
