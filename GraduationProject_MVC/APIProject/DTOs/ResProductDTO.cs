namespace ApiProject.DTOs
{
    public class ResProductDTO
    {
        public int ProductId { get; init; }
        public string? Name { get; init; }
        public int CategoryId { get; init; }
        public string? CategoryName { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public string? PrimaryImageUrl { get; init; } // 取 tProductAsset 的主圖 fIsPrimary=1 或第一張
    }
}
