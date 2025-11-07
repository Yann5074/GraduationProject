namespace ApiProject.DTOs
{
    public class ReqCompleteChatDto
    {
        public int ChatRoomId { get; set; }
        public string SenderType { get; set; } = "member"; // 只有 member/user 會走 AI
        public string Message { get; set; } = "123";
    }
}
