namespace ApiProject.DTOs
{
    public class ResChatRoomSendMessageDTO
    {
        public int MessageId { get; set; }
        public string SenderType { get; set; }  //發送者型別：visitor/member/bot/employee
        public string? SenderId { get; set; }  // 會員ID 和 員工ID 
        public string Content { get; set; }  //訊息
        public DateTime? CreatedAt { get; set; }
    }
}
