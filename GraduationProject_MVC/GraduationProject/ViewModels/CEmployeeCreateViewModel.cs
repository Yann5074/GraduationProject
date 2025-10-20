using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class CEmployeeCreateViewModel
    {
        [Required(ErrorMessage ="請輸入姓名")]
        public string FName { get; set; }

        [Required(ErrorMessage ="請輸入電話")]
        public string FPhone { get; set; }

        [Required(ErrorMessage ="請輸入電子郵件")]
        [EmailAddress(ErrorMessage ="請輸入正確的電子郵件格式")]
        public string FEmail { get; set; }

        [Required(ErrorMessage ="請輸入性別")]
        public int FGender { get; set; }

        [Required(ErrorMessage ="請輸入血型")]
        public string FBloodType { get; set; }

        [Required(ErrorMessage ="請設定入職日期")]
        public DateTime? FHireDate { get; set; }

        [Required(ErrorMessage ="請設定權限")]
        public int FRoleId { get; set; }

        [Required(ErrorMessage ="請設定狀態")]
        public int FStatusId { get; set; }

        [Required(ErrorMessage ="請輸入帳號")]
        public string FAccount { get; set; } = default!;

        [Required(ErrorMessage ="請輸入密碼")]
        public string FPasswords { get; set; } = default!;
    }
}
