namespace ApiProject.DTOs
{
    public class ResMemberUploadPhotoDTO
    {
        public string Url { get; set; }  // 完整可存取的圖片網址
        public string FileName { get; set; } // 實際存檔名（可選）
    }
}
