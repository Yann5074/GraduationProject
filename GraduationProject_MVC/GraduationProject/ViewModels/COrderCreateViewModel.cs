using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class COrderCreateViewModel
    {
        [Required]
        public int MemberId { get; set; }

        [Display(Name = "會員名稱")]
        public string? MemberName { get; set; } 

        [Required]
        public int EmployeeId { get; set; }

        [Display(Name = "負責員工")]
        public string EmployeeName { get; set; }
        [Display(Name = "總金額")]
        public decimal TotalPrice { get; set; }
        [Display(Name = "折扣")]
        public decimal Discount { get; set; }
        [Display(Name = "統編")]
        public string? TaxNo { get; set; }
        [Display(Name = "下單時間")]
        public DateTime OrderTime { get; set; }
        [Display(Name = "訂單狀態")]
        public int OrderStatus { get; set; }
        [Display(Name = "付款方式")]
        public int PaymentMethod { get; set; }
        [Display(Name = "付款狀態")]
        public int PaymentStatus { get; set; }
        [Display(Name = "付款時間")]
        public DateTime? PaymentTime { get; set; }
        [Display(Name = "取貨方式")]
        public int PickupMethod { get; set; }
        [Display(Name = "運送狀態")]
        public int DeliveryStatus { get; set; }
        [Display(Name = "運送地址")]
        public string DeliveryAddress { get; set; }
        [Display(Name = "運費")]
        public int? ShippingCost { get; set; }
        [Display(Name = "送達時間")]
        public DateTime? DeliveryTime { get; set; }
        [Display(Name = "貨運公司")]
        public int? LogisticsProvider { get; set; }
        [Display(Name = "訂單完成時間")]
        public DateTime? OrderCompletionTime { get; set; }
        [Display(Name = "訂單備註")]
        public string? Note { get; set; }
    }
}
