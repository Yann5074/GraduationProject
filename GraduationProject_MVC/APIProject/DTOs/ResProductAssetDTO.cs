namespace ApiProject.DTOs
{
    public class ResProductAssetDTO
    {
        public int AssetId { get; init; }
        public string? Url { get; init; }       // tProductAsset.fUrl
        public string? Picture { get; init; }   // fPicture
        public bool IsPrimary { get; init; }    // fIsPrimary
        public int SortOrder { get; init; }     // fSortOrder
        public string? MimeType { get; init; }  // fMimeType
    }
}
