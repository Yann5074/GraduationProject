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
    }
}
