namespace GraduationProject.DTOs
{
    public class CTextureEditDTO
    {
        public string TextureType { get; set; } = default!; // BaseColor/Normal/...
        public string FilePath { get; set; } = default!;
        public string? Tiling { get; set; } // 例：2x2
    }
}
