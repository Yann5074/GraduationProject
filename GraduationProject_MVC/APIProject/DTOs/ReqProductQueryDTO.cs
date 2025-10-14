namespace ApiProject.DTOs
{
    public class ReqProductQueryDTO
    {
        public string? Q { get; set; }
        public int? CategoryId { get; set; }
        public int? ColorId { get; set; }
        public int? Status { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; }
        public string? SortDir { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
