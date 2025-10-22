using GraduationProject.DTOs;
using GraduationProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;



namespace GraduationProject.ViewModels
{
    public class CProductEditViewModel
    {
        [Required] public int ProductId { get; set; }
        [Required, Display(Name = "商品名稱")] public string Name { get; set; } = default!;
        [Display(Name = "分類")] public int? CategoryId { get; set; }
        [Display(Name = "說明")] public string? Description { get; set; }
        [Display(Name = "狀態")] public int? PStatus { get; set; }
        public int? WarrantyMonth { get; set; }
        public bool? AssemblyRequired { get; set; }
        public string? AssemblyPart { get; set; }
        public int? Discount { get; set; }

        // 前端編輯器的 JSON（載入初始值 + 編輯後送出）
        public string? VariantsJson { get; set; }
        public string? AssetsJson { get; set; }
        public string? PartsJson { get; set; }

        // 下拉
        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public List<SelectListItem> PStatusOptions { get; set; } = new();

        public CProductUpdateDTO ToUpdateDto() => new CProductUpdateDTO
        {
            ProductId = ProductId,
            Name = Name,
            CategoryId = CategoryId,
            Description = Description,
            PStatus = PStatus,
            WarrantyMonth = WarrantyMonth,
            AssemblyRequired = AssemblyRequired,
            AssemblyPart = AssemblyPart,
            Discount = Discount
            // 子集合由 Controller 反序列化填入
        };
    }
}
