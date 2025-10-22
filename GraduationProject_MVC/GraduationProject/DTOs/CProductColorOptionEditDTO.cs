using GraduationProject.ViewModels;

namespace GraduationProject.DTOs
{
    public class CProductColorOptionEditDTO
    {
        public int? ColorOptionId { get; set; }
        public string? OptionName { get; set; }
        public int? DisplayOrder { get; set; }
        public List<CTextureEditDTO> Textures { get; set; } = new List<CTextureEditDTO>();
    }
}
