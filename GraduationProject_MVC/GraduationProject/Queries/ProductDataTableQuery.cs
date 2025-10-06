namespace GraduationProject.Queries
{
    public record ProductDataTableQuery
    {
        public string? Search { get; init; }
        public string SortField { get; init; } = "ProductId";
        public string SortDir { get; init; } = "ASC"; // or DESC
        public int Start { get; init; } = 0;
        public int Length { get; init; } = 10;
    }
}
