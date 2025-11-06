namespace ApiProject.DTOs
{
    public class ReqSendMessageDTO
    {
       public int ChatRoomId {  get; set; }
       public string Content { get; set; }
        public string SenderType { get; set; } = "member"; // member / user / employee

    }
}
