namespace GraduationProject.ViewModels
{
    public class COrderDashboardViewModel
    {
        // 每月營收
        public List<string> MonthlyLabels { get; set; } = new();
        public List<decimal> MonthlySales { get; set; } = new();

        // 訂單狀態分佈
        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusCounts { get; set; } = new();

        // 付款方式分佈
        public List<string> PaymentLabels { get; set; } = new();
        public List<int> PaymentCounts { get; set; } = new();
    }
}
