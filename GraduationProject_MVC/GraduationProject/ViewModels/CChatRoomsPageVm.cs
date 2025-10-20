using GraduationProject.DTOs;

namespace GraduationProject.ViewModels
{
    /// <summary>
    /// CChatRoomsPageVm = 這個VM 就是把聊天室需要的 左側 的 ChatRoomList 和 右側的 MessageList 以及選擇了的ChatRoomId
    /// VM只是傳遞資料的媒介 controller 傳到 view的 箱子
    /// </summary>

    public class CChatRoomsPageVm
    {
        //畫面 左側 的 ChatRoomList  名稱為Rooms
        public List<ChatRoomListDTO> Rooms { get; set; } = new();

        //選擇了的ChatRoomId  
        public int? SelectedId { get; set; }

        //畫面右側的 MessageList 名稱為Messages
        public List<MessageDto> Messages { get; set; } = new();

        public string Q { get; set; } //查詢關鍵字
    }
}
