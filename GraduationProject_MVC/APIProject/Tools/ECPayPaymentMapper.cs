namespace ApiProject.Tools
{
    public static class ECPayPaymentMapper
    {
        public static string ToECPayCode(int paymentMethodId)
        {
            return paymentMethodId switch
            {
                1 => "Cash",
                2 => "Credit",
                3 => "WebATM",
                4 => "ATM",
                5 => "CVS",
                6 => "BARCODE"
            };
        }
    }
}
