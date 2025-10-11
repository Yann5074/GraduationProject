namespace GraduationProject.ViewModels
{
    public class CProductPictureViewModel
    {
        public bool IsPrimary { get; set; }     // 使用者在預覽區選的主圖
        public int? SortOrder { get; set; }     // 使用者在預覽區輸入的排序
    }
}
