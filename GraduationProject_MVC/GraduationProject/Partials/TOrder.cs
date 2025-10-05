namespace GraduationProject.Models
{
    public partial class TOrder
    {
        //用於 Navigation Property
        public TMember Member { get; set; }
        public TEmployee Employee { get; set; }
        public TOrderStatus OrderStatus { get; set; }
        public TPaymentStatus PaymentStatus { get; set; }
        public TDeliveryStatus DeliveryStatus { get; set; }
        public TLogisticsProvider LogisticsProvider { get; set; }
    }
}
