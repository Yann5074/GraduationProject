using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class OrderCreateDTO
    {
        public int MemberId { get; set; }

        public int EmployeeId { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal Discount { get; set; }

        public string? TaxNo { get; set; }

        public DateTime OrderTime { get; set; }

        public int OrderStatus { get; set; }

        public int PaymentMethod { get; set; }

        public int PaymentStatus { get; set; }

        public DateTime? PaymentTime { get; set; }

        public int PickupMethod { get; set; }

        public int DeliveryStatus { get; set; }

        public string DeliveryAddress { get; set; }

        public int? ShippingCost { get; set; }

        public DateTime? DeliveryTime { get; set; }

        public int? LogisticsProvider { get; set; }

        public DateTime? OrderCompletionTime { get; set; }

        public string? Note { get; set; }
    }
}
