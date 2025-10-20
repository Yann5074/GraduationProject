namespace ApiProject.DTOs
{
    public class ResProductAssetDTO
    {
        public int FAssetId { get; set; }
        public int? FProductId { get; set; }
        public int? FProductVariantId { get; set; }
        public string? FPicture { get; set; }
        public string? FAssetType { get; set; }
        public string? FMimeType { get; set; }
        public string? FUrl { get; set; }
        public string? FFilePath { get; set; }
        public string? FPosterUrl { get; set; }
        public bool? FIsPrimary { get; set; }
        public int? FSortOrder { get; set; }
    }
}
