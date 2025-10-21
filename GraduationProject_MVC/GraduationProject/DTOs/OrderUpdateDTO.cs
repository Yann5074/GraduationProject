using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class OrderUpdateDTO
    {
        //確認是否可找尋到此訂單的狀態紀錄
        public bool isValid { get; set; }

        public int OrderId { get; set; }

        public string? EmployeeName { get; set; }

        public decimal? Discount { get; set; }

        public int OrderStatus { get; set; }

        public int PaymentStatus { get; set; }

        public int PickupMethod { get; set; }

        public int DeliveryStatus { get; set; }

        public string? DeliveryAddress { get; set; }

        public decimal? ShippingCost { get; set; }

        public DateTime? DeliveryTime { get; set; }

        public int? LogisticsProvider { get; set; }

        public DateTime? OrderCompletionTime { get; set; }

        public string? FNote { get; set; }
    }
}
