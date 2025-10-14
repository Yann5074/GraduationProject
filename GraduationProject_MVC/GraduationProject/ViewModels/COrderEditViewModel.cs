using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class COrderEditViewModel
    {
        [Display(Name = "訂單編號")]
        public int OrderId { get; set; }
        [Display(Name = "負責員工")]
        public string? EmployeeName { get; set; }
        [Display(Name = "折扣")]
        public decimal? Discount { get; set; }
        [Display(Name = "訂單狀態")]
        public int OrderStatus { get; set; }
        [Display(Name = "付款狀態")]
        public int PaymentStatus { get; set; }
        [Display(Name = "取貨方式")]
        public int PickupMethod { get; set; }
        [Display(Name = "運送狀態")]
        public int DeliveryStatus { get; set; }
        [Display(Name = "運送地址")]
        public string? DeliveryAddress { get; set; }
        [Display(Name = "運費")]
        public int? ShippingCost { get; set; }
        [Display(Name = "送達時間")]
        public DateTime? DeliveryTime { get; set; }
        [Display(Name = "貨運公司")]
        public int? LogisticsProvider { get; set; }
        [Display(Name = "訂單完成時間")]
        public DateTime? OrderCompletionTime { get; set; }
        [Display(Name = "訂單備註")]
        public string? FNote { get; set; }
    }
}
