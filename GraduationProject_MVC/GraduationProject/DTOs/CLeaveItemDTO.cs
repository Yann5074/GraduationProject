namespace GraduationProject.DTOs
{
    public class CLeaveItemDTO
    {
        public int LeaveId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string LeaveType { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = "";
        public string? PictureFileName { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
