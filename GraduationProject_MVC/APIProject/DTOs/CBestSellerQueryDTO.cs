namespace ApiProject.DTOs
{
    public class CBestSellerQueryDTO
    {
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }
        public int Top { get; init; } = 5;
        /// 逗號字串，例如 "2,3,4,5"
        public string? Statuses { get; init; }
    }
}
