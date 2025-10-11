namespace GraduationProject.ViewModels
{
    public class CProductAssetEditItem
    {
        public int AssetId { get; set; }       
        public string FPicture { get; set; }        
        public bool IsPrimary { get; set; }
        public int? SortOrder { get; set; }    
        public string AssetType { get; set; }  
    }
}
