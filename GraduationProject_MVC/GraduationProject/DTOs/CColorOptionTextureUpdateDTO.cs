namespace GraduationProject.DTOs
{
    public class CColorOptionTextureUpdateDTO
    {
        public int? TextureId { get; set; } // null = 新增
        public bool? Deleted { get; set; }
        public string TextureType { get; set; } = default!;
        public string FilePath { get; set; } = default!;
        public string? Tiling { get; set; }
    }
}
