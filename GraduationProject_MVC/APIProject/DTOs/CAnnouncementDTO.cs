namespace ApiProject.DTOs
{
    public class CAnnouncementDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public DateTime LastUpdated { get; set; }

        // 若未用併發，可忽略（MVC 仍可兼容）
        //public byte[]? RowVersion { get; set; }
    }
}
