using GraduationProject.DTOs;

namespace GraduationProject.ViewModels
{
    public class CChatRoomsPageVm
    {
        public List<ChatRoomListDTO> Rooms { get; set; } = new();

        public int? SelectedId { get; set; }

        public List<MessageDto> Messages { get; set; } = new();

        public string Q { get; set; } //查詢關鍵字
    }
}
