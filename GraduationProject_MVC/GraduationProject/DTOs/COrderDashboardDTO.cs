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
    }
}
