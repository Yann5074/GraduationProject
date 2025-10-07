namespace GraduationProject.DTOs
{
    public class CMemberListPageDTO
    {
        public required IReadOnlyList<CMemberDTO> Items { get; init; }
        public required int Page { get; init; }
        public required int PageSize { get; init; }
        public  int TotalCount { get; init; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
