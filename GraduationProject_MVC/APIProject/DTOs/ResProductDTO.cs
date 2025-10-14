namespace ApiProject.DTOs
{
    public class ResProductDTO
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public int? CategoryId { get; set; }
        public int? PStatus { get; set; }
        public string? PStatusName { get; set; }
        public decimal PriceMin { get; set; }
        public decimal PriceMax { get; set; }
        public int TotalStock { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
