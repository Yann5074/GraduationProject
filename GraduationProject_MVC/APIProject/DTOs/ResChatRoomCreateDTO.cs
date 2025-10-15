namespace ApiProject.DTOs
{
    public class ResChatRoomCreateDTO
    {
        public int FChatRoomId { get; set; }
        public string? image { get; set; }  //會員投向
        public string? FVisitorId { get; set; }  //訪客ID
        public string FName { get; set; }  //會員名稱
        public string FStatus { get; set; }  //聊天室狀態
        public string  Fcontent { get; set; }  //訊息
        public string? FEmployeeId { get; set; }  //員工ID
        public DateTime FCreatedAt { get; set; }  //建立時間
        public DateTime? FClosedAt { get; set; }  //關閉時間
    }
}
