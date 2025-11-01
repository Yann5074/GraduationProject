namespace ApiProject.DTOs
{
    public class ReqECPayDTO
    {
        public string MerchantTradeNo { get; set; }

        public int TotalAmount { get; set; }

        public int PaymentMethodId { get; set; }
    }
}