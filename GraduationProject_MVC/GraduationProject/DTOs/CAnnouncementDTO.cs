namespace GraduationProject.DTOs
{
    // 後台顯示／編輯公告用 DTO（由 API 取得）
    public class CAnnouncementDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public DateTime LastUpdated { get; set; }
        //public byte[]? RowVersion { get; set; }
    }
}
