using GraduationProject.DTOs;

namespace GraduationProject.ViewModels
{
    public class ChatRoomsPageVm
    {
        public List<ChatRoomLlistDTO> Rooms { get; set; } = new();

        public int? SelectedId { get; set; }

        public List<MessageDto> Messages { get; set; } = new();
    }
}
