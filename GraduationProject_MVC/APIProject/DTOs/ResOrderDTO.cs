using System.ComponentModel.DataAnnotations;

namespace ApiProject.DTOs
{
    public class ResOrderDTO
    {
        public string OrderId { get; set; }
        public int IsDeleted { get; set; }
        public string MemberName { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string OrderTime { get; set; }
        public int OrderStatusId { get; set; }
        public string OrderStatus { get; set; }
        public int PaymentStatusId { get; set; }
        public string PaymentStatus { get; set; }
        public int DeliveryStatusId { get; set; }
        public string DeliveryStatus { get; set; }
    }
}