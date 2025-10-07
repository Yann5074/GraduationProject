namespace GraduationProject.DTOs
{
    public class OrderDetailUpdateDTO
    {
        public bool isValid { get; set; }

        public int OrderId { get; set; }

        public int ProductVariantId { get; set; }

        public string ProductName { get; set; }

        public int IsDeleted { get; set; }

        public int Quantity { get; set; }

        public int UnitPrice { get; set; }
    }
}
