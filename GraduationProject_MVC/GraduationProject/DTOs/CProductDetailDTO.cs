using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class CProductDetailDTO : CProductDTO
    {
       
            public int ProductId { get; set; }

            [Display(Name = "產品名稱")]
            public string? Name { get; set; }

            [Display(Name = "產品描述")]
            public string? Description { get; set; }

            public int? CategoryId { get; set; }

            [Display(Name = "種類")]
            public string? CategoryName { get; set; }

            public int? PStatusId { get; set; }

            public int? PStatus { get; set; }

            [Display(Name = "狀態")]
            public string? PStatusName { get; set; }

            public decimal? PriceMin { get; set; }
            public decimal? PriceMax { get; set; }
            public decimal? CostMin { get; set; }
            public decimal? CostMax { get; set; }

            public List<string> Colors { get; set; } = new();
            public List<CProductImageDTO> Images { get; set; } = new();
            public string? MainImageUrl => Images.FirstOrDefault()?.Url; // 以排序後第一張為主圖

            public int? WarrantyMonth { get; set; }        // fWarrantyMonth
            public bool? AssemblyRequired { get; set; }    // fAssemblyRequired
            public string AssemblyPart { get; set; }       // fAssemblyPart


            public decimal? LengthMin { get; set; }
            public decimal? LengthMax { get; set; }
            public decimal? WidthMin { get; set; }
            public decimal? WidthMax { get; set; }
            public decimal? HeightMin { get; set; }
            public decimal? HeightMax { get; set; }
            public decimal? WeightMin { get; set; }
            public decimal? WeightMax { get; set; }

            public int? StockTotal { get; set; }
    }
}
