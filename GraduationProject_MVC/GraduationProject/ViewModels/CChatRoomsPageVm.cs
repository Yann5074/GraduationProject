using GraduationProject.DTOs;

namespace GraduationProject.ViewModels
{
    public class CChatRoomsPageVm
    {
        public List<ChatRoomLlistDTO> Rooms { get; set; } = new();

        public int? SelectedId { get; set; }

        public List<MessageDto> Messages { get; set; } = new();
    }
}
