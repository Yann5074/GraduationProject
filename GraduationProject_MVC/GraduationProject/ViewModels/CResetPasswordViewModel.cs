using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class CResetPasswordViewModel
    {
        [Required] 
        public string Token { get; set; } = default!;
        [Required] 
        public string Account { get; set; } = default!;

        [Required, StringLength(100, MinimumLength = 8,
            ErrorMessage = "密碼至少 8 碼")]
        [DataType(DataType.Password)]
        // 可用 Regex 強化：大小寫/數字/符號
        public string NewPassword { get; set; } = default!;

        [DataType(DataType.Password), Compare(nameof(NewPassword),
            ErrorMessage = "兩次輸入的密碼不一致")]
        public string ConfirmPassword { get; set; } = default!;
    }
}
