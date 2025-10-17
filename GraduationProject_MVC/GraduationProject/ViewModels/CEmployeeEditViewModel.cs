using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class CEmployeeEditViewModel
    {
        public int EmployeeId { get; set; }
        public string? FName { get; set; }
        public string? FPhone { get; set; }
        public string? FEmail { get; set; }
        public string? FHeadShot { get; set; }
        public int? FGender { get; set; }
        public string? FBloodType { get; set; }
        public DateTime? FHireDate { get; set; }
        public int? FRoleId { get; set; }
        public int? FStatusId { get; set; }
        public string? FAccount { get; set; } = default!;
        [StringLength(500)]
        public string? FPasswords { get; set; } = default!;
        public string? NewPassword { get; set; }
        [Compare(nameof(NewPassword), ErrorMessage = "密碼與確認密碼不一致")]
        public string? ConfirmPassword { get; set; }
        public DateTime? FLoginTime { get; set; }
        public DateTime? FChangePasswordTime { get; set; }
        public IFormFile? Photo { get; set; }
        public bool FromDeleted { get; set; }
    }
}
