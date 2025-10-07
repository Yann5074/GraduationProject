namespace GraduationProject.DTOs
{
    public class CEmployeeListPageDTO
    {
        public required IReadOnlyList<CEmployeeListItemDTO> Items { get; init; }
        public required int Page { get; init; }
        public required int PageSize { get; init; }
        public required int Total { get; init; }
        public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
        public string? Keyword { get; init; }
    }
}
