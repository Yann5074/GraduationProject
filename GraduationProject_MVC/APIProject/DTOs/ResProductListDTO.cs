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
        public DateTime? FCreateTime { get; set; }
        public DateTime? FUpdateTime { get; set; }
    }
}
