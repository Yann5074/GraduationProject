namespace GraduationProject.DTOs
{
    public class CProductDTO
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? PStatus { get; set; }
        public string? ColorName { get; set; }
        public string FPStatusName { get; set; }
        public decimal? Price { get; set; }

        public decimal? Cost { get; set; }
        public int? Discount { get; set; }

        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }

    }
}
