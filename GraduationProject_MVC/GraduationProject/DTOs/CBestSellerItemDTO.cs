namespace GraduationProject.DTOs
{
    public class CBestSellerItemDTO
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }

        public int? VariantId { get; set; }
        public string? SKU { get; set; }

        public int TotalQty { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
