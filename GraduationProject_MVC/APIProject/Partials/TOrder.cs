using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TOrder
    {
        //用於 Navigation Property 與宣告外鍵
        [ForeignKey(nameof(FOrderId))]
        public TOrderDetail OrderDetail { get; set; }

        [ForeignKey(nameof(FMemberId))]
        public TMember Member { get; set; }

        [ForeignKey(nameof(FEmployeeId))]
        public TEmployee? Employee { get; set; }

        [ForeignKey(nameof(FOrderStatus))]
        public TOrderStatus OrderStatus { get; set; }

        [ForeignKey(nameof(FPaymentStatus))]
        public TPaymentStatus PaymentStatus { get; set; }

        [ForeignKey(nameof(FDeliveryStatus))]
        public TDeliveryStatus DeliveryStatus { get; set; }

        [ForeignKey(nameof(FLogisticsProvider))]
        public TLogisticsProvider LogisticsProvider { get; set; }
    }
}
