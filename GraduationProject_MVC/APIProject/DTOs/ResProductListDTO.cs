namespace ApiProject.DTOs
{
    public class ResProductListDTO
    {

        public int FProductId { get; set; }
        public string FName { get; set; }
        public string FDescription { get; set; }
        public int? FCategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? FPstatus { get; set; }
        public string StatusName { get; set; }
        public int? FWarrantyMonth { get; set; }
        public bool? FAssemblyRequired { get; set; }
        public int? FDiscount { get; set; }
        public string MainImageUrl { get; set; }
        public int TotalStock { get; set; }
        public bool IsAvailable { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        // 是否為可自訂產品（有部位選項）
        public bool IsCustomizable { get; set; }
        // 可自訂部位數量
        public int CustomizablePartsCount { get; set; }
        // 可用的顏色組合數量
        public int AvailableCombinationsCount { get; set; }
        public DateTime? FCreateTime { get; set; }
        public DateTime? FUpdateTime { get; set; }
    }
}
