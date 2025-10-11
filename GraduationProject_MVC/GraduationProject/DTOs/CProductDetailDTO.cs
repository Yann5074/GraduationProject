namespace GraduationProject.DTOs
{
    public class CProductDetailDTO
    {
       
            public int? ProductId { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }

            public int? CategoryId { get; set; }
            public string? CategoryName { get; set; }

            public int? PStatusId { get; set; }
            public string? PStatusName { get; set; }

            public decimal? PriceMin { get; set; }
            public decimal? PriceMax { get; set; }
            public decimal? CostMin { get; set; }
            public decimal? CostMax { get; set; }

            public List<string> Colors { get; set; } = new();
            public List<CProductImageDTO> Images { get; set; } = new();
            public string? MainImageUrl => Images.FirstOrDefault()?.Url; // 以排序後第一張為主圖
        
    }
}
