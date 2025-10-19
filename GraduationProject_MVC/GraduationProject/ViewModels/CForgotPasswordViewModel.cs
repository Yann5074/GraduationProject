using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class CForgotPasswordViewModel
    {
        [Required] 
        public string Account { get; set; } = default!;
        [Required, EmailAddress] 
        public string Email { get; set; } = default!;
    }
}
