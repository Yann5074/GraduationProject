namespace GraduationProject.ViewModels
{
    public class ChatRoomLlistVM
    {

        public string? image { get; set; } //會員頭像

        public int FChatRoomId { get; set; }// 看不到的聊天室

        public string FName { get; set; } //會員名稱跟玉祥拿  寫判斷  訪客名稱 要創建聊天室時限制他

        public DateTime? FLastMessageTime { get; set; } //message 資料表 最後時間

        public string FLastMessage { get; set; } //message 資料表 最後訊息




    }
}
