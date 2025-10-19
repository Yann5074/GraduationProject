namespace ApiProject.DTOs
{
    public class ResCartItemDTO
    {
        public int CartItemId { get; set; }
        public int ProductVariantId { get; set; }

        public string ProductName { get; set; }

        public decimal UnitPrice { get; set; }

        public int Qty { get; set; }

        public decimal SubTotal { get; set; }

        public string ImageUrl { get; set; }

        public string Size { get; set; }
    }
}
