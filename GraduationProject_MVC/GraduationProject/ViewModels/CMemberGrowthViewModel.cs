namespace GraduationProject.ViewModels
{
    public class CMemberGrowthViewModel
    {
        // X 軸：yyyy-MM
        public List<string> Labels { get; set; } = new();
        // 每月新註冊會員數
        public List<int> NewMembers { get; set; } = new();
        // 月增率（相對上月）：百分比（e.g. 25.0 代表 +25%）
        public List<decimal> GrowthRates { get; set; } = new();
        // 新增：總會員數（全庫）
        public int TotalMembers { get; set; }
        // （可選）啟用中會員數（例如 FStatus == 1）
        public int ActiveMembers { get; set; }
    }
}

