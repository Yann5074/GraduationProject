namespace ApiProject.DTOs
{
    public class ReqSendMessageDTO
    {
       public int chatRoomId {  get; set; }
       public string content { get; set; }
       public string? visitorId {  get; set; }
       public int? employeeId { get; set; }
    }
}
