namespace GraduationProject.DTOs
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public string SenderType { get; set; }  //發送者型別：visitor/agent/bot/system
        public string? SenderId { get; set; }  // 會員ID 和 員工ID 吧
        public string Content { get; set; }  //訊息
        public DateTime? CreatedAt { get; set; }
    }
}
