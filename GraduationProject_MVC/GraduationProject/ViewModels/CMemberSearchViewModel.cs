using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class CMemberSearchViewModel
    {
        [Display(Name = "會員電話")]
        [Required(ErrorMessage ="請輸入會員手機號碼")]
        public string MemberPhone { get; set; }
    }
}
