namespace GraduationProject.DTOs
{
    public class CEmployeeCreateDTO
    {
        public string FName { get; set; } = default!;
        public string? FPhone { get; set; }
        public string? FEmail { get; set; }
        public int? FGender { get; set; }
        public string? FBloodType { get; set; }
        public DateTime? FHireDate { get; set; }
        public int? FRoleId { get; set; }
        public int? FStatusId { get; set; }
        public string FAccount { get; set; } = default!;
        public string FPasswords { get; set; } = default!;
        public string? HeadShotFileName { get; set; } = "default.png"; 
    }
}
