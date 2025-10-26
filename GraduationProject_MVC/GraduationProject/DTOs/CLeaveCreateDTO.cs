namespace GraduationProject.DTOs
{
    public class CLeaveCreateDTO
    {
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public string? PictureFileName { get; set; }
    }
}
