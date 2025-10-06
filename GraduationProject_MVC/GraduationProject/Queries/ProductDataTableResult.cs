using GraduationProject.DTOs;

namespace GraduationProject.Queries
{
    public record ProductDataTableResult
    {
        public int TotalRecords { get; init; }
        public int FilteredRecords { get; init; }
        public IReadOnlyList<CProductDTO> Items { get; init; } = Array.Empty<CProductDTO>();
    }
}
