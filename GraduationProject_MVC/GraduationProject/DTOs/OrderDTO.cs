namespace GraduationProject.DTOs
{
    public class OrderDTO
    {
        public string OrderId { get; set; }
        public string MemberName { get; set; }
        public string? EmployeeName { get; set; }
        public string OrderTime { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string DeliveryStatus { get; set; }
        public string? LogisticsProvider { get; set; }
        public string Note {  get; set; }

    }
}