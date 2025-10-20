namespace ApiProject.DTOs
{
    internal class ResChatRoomListDTO
    {
        public int FChatRoomId { get; set; }
        public string image { get; set; }
        public string FName { get; set; }
        public DateTime? FLastMessageTime { get; set; }
        public string FLastMessage { get; set; }
    }
}