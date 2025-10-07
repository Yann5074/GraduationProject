using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class OrderSearchDTO
    {
        [Display(Name = "訂單編號")]
        [Required]
        public string OrderId { get; set; }
        [Display(Name = "會員姓名")]
        [Required]
        public string MemberName { get; set; }
        [Display(Name = "會員電話")]
        [Required]
        public string MemberPhone { get; set; }
        [Display(Name = "負責員工")]
        public string? EmployeeName { get; set; }
        [Display(Name = "下單時間")]
        [Required]
        public string OrderTime { get; set; }
        [Display(Name = "訂單狀態")]
        [Required]
        public string OrderStatus { get; set; }
        [Display(Name = "付款狀態")]
        [Required]
        public string PaymentStatus { get; set; }
        [Display(Name = "運送狀態")]
        [Required]
        public string DeliveryStatus { get; set; }
        [Display(Name = "運送廠商")]
        public string? LogisticsProvider { get; set; }
        [Display(Name = "備註")]
        public string Note {  get; set; }

    }
}