namespace ApiProject.DTOs
{
    internal class ResMessageDto
    {
        public int MessageId { get; set; }
        public string SenderType { get; set; }
        public string SenderId { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        // ★ 新增：為了讓前端能渲染卡片/解析資訊
        public object? Parsed { get; set; }
        public List<ProductPickDto>? Items { get; set; }
    }
}