namespace ApiProject.DTOs
{
    internal class ResChatRoomsPageDTO
    {
        public List<ResChatRoomListDTO> Rooms { get; set; }
        public int? SelectedId { get; set; }
        public List<ResMessageDto> Messages { get; set; }
    }
}