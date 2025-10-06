namespace GraduationProject.DTOs
{
    public class CProductListItemDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }

        public int TotalStock { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Price { get; set; }

        public string FImagePath { get; set; }
        public string FirstSku { get; set; }
        public int? FirstColorId { get; set; }

        
        public List<string> Skus { get; set; } = new();
        public string AllSkus => string.Join(",", Skus);

        public int? MaterialId { get; set; }
        public int? TexturedId { get; set; }
        public int? ModelId { get; set; }

        public DateTime? FCreateTime { get; set; }
        public DateTime? FUpdateTime { get; set; }
        public bool? FPStatus { get; set; }
        public decimal? FDiscount { get; set; }
    }
}
