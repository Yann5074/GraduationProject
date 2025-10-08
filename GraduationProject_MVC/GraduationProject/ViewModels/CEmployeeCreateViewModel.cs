using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class CEmployeeCreateViewModel
    {
        [Required, StringLength(50)]
        public string FName { get; set; }
        [Required, StringLength(10)]
        public string FPhone { get; set; }
        [Required, EmailAddress, StringLength(30)]
        public string FEmail { get; set; }
        public int FGender { get; set; }
        public string FBloodType { get; set; }
        public DateTime? FHireDate { get; set; }
        public int FRoleId { get; set; }
        public int FStatusId { get; set; }
        public string FAccount { get; set; } = default!;
        public string FPasswords { get; set; } = default!;
    }
}
