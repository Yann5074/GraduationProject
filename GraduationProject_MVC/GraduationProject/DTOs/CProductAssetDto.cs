namespace GraduationProject.DTOs
{
    public class CProductAssetDTO
    {
        public int? AssetId { get; set; }
        public int? ProductVariantId { get; set; } 
        public int? VariantTempIndex { get; set; }
        public string? AssetType { get; set; }     
        public string? MimeType { get; set; }
        public string? Url { get; set; }
        public bool? IsPrimary { get; set; }
        public int? SortOrder { get; set; }
        public string? PosterUrl { get; set; }
        public int? MaterialId { get; set; }
        public int? TexturedId { get; set; }
        public int? ModelId { get; set; }
        public string? MetadateJson { get; set; }
        public bool? Deleted { get; set; }
    }
}
