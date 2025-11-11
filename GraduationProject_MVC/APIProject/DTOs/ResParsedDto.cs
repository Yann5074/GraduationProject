namespace ApiProject.DTOs.Responses
{
    public sealed class ParsedDto
    {
        public string? Category { get; set; }
        public string? Color { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
    }
}
