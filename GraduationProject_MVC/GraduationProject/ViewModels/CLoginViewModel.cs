using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class CLoginViewModel
    {
        [Required(ErrorMessage = "帳號必填")]
        public string txtAccount { get; set; }

        [Required(ErrorMessage = "密碼必填")]
        public string txtPassword { get; set; }
    }
}
