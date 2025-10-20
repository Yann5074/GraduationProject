namespace ApiProject.DTOs
{
    internal class ResMessageDto
    {
        public int MessageId { get; set; }
        public string SenderType { get; set; }
        public string SenderId { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}