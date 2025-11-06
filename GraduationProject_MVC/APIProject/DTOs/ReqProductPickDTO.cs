namespace ApiProject.DTOs
{
    /// <summary>
    /// 給 AI 或商品查詢回傳使用的 DTO
    /// </summary>
    public class ProductPickDto
    {
        public int? ProductId { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Color { get; set; } = "";
        public decimal? Price { get; set; }
    }
}