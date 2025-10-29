namespace ApiProject.DTOs
{
    public class ReqCreateOrderDTO
    {
        public int MemberId {  get; set; }

        public string ContactName { get; set; }

        public string ContactPhone { get; set; }

        public int? EmployeeId { get; set; }

        public string? TaxNo { get; set; }

        public int PaymentMethod {  get; set; }

        public int PickupMethod { get; set; }

        public string? DeliveryAddress { get; set; }

        public decimal? ShippingCost { get; set; }

        public int? LogisticsProvider { get; set; }

        public string? Note {  get; set; }

    }
}
