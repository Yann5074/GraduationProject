namespace ApiProject.DTOs
{
    public class ReqProductFilterDTO
    {
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? StatusId { get; set; }
        public string? Keyword { get; set; }
        public string? SortBy { get; set; } = "created_desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;

    }
}
