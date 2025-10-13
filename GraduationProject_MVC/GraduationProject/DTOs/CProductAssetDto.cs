namespace GraduationProject.DTOs
{
    public class CProductAssetDTO
    {
        public IFormFile Upload { get; set; }

        public string? AssetType { get; set; }
        public string? MimeType { get; set; }
        public int? MaterialId { get; set; }
        public int? TexturedId { get; set; }
        public int? ModelId { get; set; }
        public string? PosterUrl { get; set; }
        public bool IsPrimary { get; set; }
        public int? SortOrder { get; set; }
    }
}
