namespace GraduationProject.ViewModels
{
    public class CLeaveListItemViewModel
    {
        public int LeaveId { get; set; }
        public string LeaveType { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; } = "";
        public string? PictureUrl { get; set; }
        public string? Description { get; set; }
        public DateTime? CreateTime { get; set; }

        //篩選用
        public string? Keyword { get; set; } // 關鍵字
        public DateTime? Start { get; set; } // 起
        public DateTime? End { get; set; }   // 迄

        //分頁
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
    }
}
