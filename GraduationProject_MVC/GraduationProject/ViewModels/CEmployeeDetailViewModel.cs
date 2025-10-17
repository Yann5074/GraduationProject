namespace GraduationProject.ViewModels
{
    public class CEmployeeDetailViewModel
    {
        public int FEmployeeId { get; set; }
        public string? FName { get; set; }
        public string? FPhone { get; set; }
        public string? FEmail { get; set; }
        public string? FHeadShot { get; set; }
        public string? FGenderName { get; set; }
        public string? FBloodType { get; set; }
        public DateTime? FHireDate { get; set; }
        public string? FRoleClass { get; set; }
        public string? FRoleBadgeClass { get; set; }
        public string? FStatus { get; set; }
        public string? FStatusBadgeClass { get; set; }
        public string? FAccount { get; set; }
        public DateTime? FLoginTime { get; set; }
        public DateTime? FChangePasswordTime { get; set; }
        public bool FromDeleted { get; set; }
    }
}
