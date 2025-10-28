using GraduationProject.DTOs;

namespace GraduationProject.ViewModels
{
    public class CLeaveRequestItemViewModel
    {
        public IEnumerable<CLeaveItemDTO> LeaveRequests { get; set; }
        public int TotalRequests { get; set; }

        //篩選用
        public string? Keyword { get; set; } // 關鍵字
        public DateTime? Start { get; set; } // 起
        public DateTime? End { get; set; }   // 迄

        //分頁
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
    }
}
