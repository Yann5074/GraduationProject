namespace GraduationProject.DTOs
{
    public class COrderDashboardDTO
    {
        public List<string> MonthlyLabels { get; set; } = new();
        public List<decimal> MonthlySales { get; set; } = new();
        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusCounts { get; set; } = new();
        public List<string> PaymentLabels { get; set; } = new();
        public List<int> PaymentCounts { get; set; } = new();

        public List<MonthCountItemDTO> MaxCompletedMonths { get; set; } = new();
        public List<MonthCountItemDTO> MinCompletedMonths { get; set; } = new();

        public List<MonthCountItemDTO> MaxCanceledMonths { get; set; } = new();
        public List<MonthCountItemDTO> MinCanceledMonths { get; set; } = new();
    }
    // 「某月份的筆數」
    public class MonthCountItemDTO
    {
        public string Label { get; set; } = ""; // e.g. "2025-09"
        public int Count { get; set; }
    }
}
