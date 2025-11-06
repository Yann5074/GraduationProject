namespace ApiProject.DTOs.Responses
{
    public sealed class ResCompleteResult
    {
        public ParsedDto Parsed { get; set; } = new();
        public List<ProductPickDto> Items { get; set; } = new();
        public string BotText { get; set; } = "";
    }
}
