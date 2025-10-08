using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class CMemberCreateViewModel
    {
        [Required(ErrorMessage = "姓名必填")]
        public string? FName { get; set; }

        [Required(ErrorMessage = "暱稱必填")]
        public string? FDisplayName { get; set; }

        [Required(ErrorMessage = "性別必填")]
        public int? FGender { get; set; }

        [Required(ErrorMessage = "電話必填")]
        public string? FPhone { get; set; }

        public DateOnly? FBirthDate { get; set; }

        [Required(ErrorMessage = "地址必填")]
        public string? FAddress { get; set; }

        //public string? FMemberImage { get; set; }

        public int? FLeveId { get; set; }

        public int? FMoneySum { get; set; }

        public int? FStatus { get; set; }
    }
}
