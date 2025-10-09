namespace GraduationProject.ViewModels
{
    public class CProductAssetEditItem
    {
        public int AssetId { get; set; }       // PK
        public string Url { get; set; }        // fUrl / 或 fPicture 二選一
        public bool IsPrimary { get; set; }
        public int? SortOrder { get; set; }    // fSortOrder
        public string AssetType { get; set; }  // 類型（image/…）可選
    }
}
