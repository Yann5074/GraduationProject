using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class CResetPasswordViewModel
    {
        [Required] 
        public string Token { get; set; } = default!;
        [Required] 
        public string Account { get; set; } = default!;

        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).{8,}$",
        ErrorMessage = "密碼至少 8 碼，且需包含大小寫字母、數字與符號")]
        public string NewPassword { get; set; } = default!;

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "密碼不一致")]
        public string ConfirmPassword { get; set; } = default!;
    }
}
