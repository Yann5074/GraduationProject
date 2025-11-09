namespace ApiProject.DTOs
{
    public class BotPayload
    {
        public string? content { get; set; }
        public object? parsed { get; set; }
        public List<ProductPickDto>? items { get; set; }
    }
}
