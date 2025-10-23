namespace ApiProject.DTOs
{
    public class ResOrderDetailDTO
    {

        public string ProductName { get; set; }

        public string ProductInfo { get; set; }

        public int IsDeleted { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public string ImageUrl { get; set; }
    }
}