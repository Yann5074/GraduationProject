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

        // 完成 最高/最低月份清單（可能有並列）
        public List<MonthCountItemVM> MaxCompletedMonths { get; set; } = new();
        public List<MonthCountItemVM> MinCompletedMonths { get; set; } = new();

        //棄單 最高/最低月份清單（可能有並列）
        public List<MonthCountItemVM> MaxCanceledMonths { get; set; } = new();
        public List<MonthCountItemVM> MinCanceledMonths { get; set; } = new();
    }
    public class MonthCountItemVM
    {
        public string Label { get; set; } = "";
        public int Count { get; set; }
    }
}
