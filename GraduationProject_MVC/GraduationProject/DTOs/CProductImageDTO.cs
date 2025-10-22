namespace GraduationProject.DTOs
{
    public class CProductImageDTO
    {
        public string? Url { get; set; }
        public bool? IsPrimary { get; set; }
        public int? SortOrder { get; set; }

        public int AssetId { get; set; }
        public string DisplayUrl { get; set; }
    }
}
