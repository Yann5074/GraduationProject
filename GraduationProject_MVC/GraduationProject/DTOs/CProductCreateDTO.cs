using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    public class CProductCreateDTO
    {
        [Required(ErrorMessage = "產品名稱為必填")]
        [StringLength(100, ErrorMessage = "產品名稱最多100字")]
        [Display(Name = "產品名稱")]
        public string Name { get; set; }

        [Required(ErrorMessage = "請選擇分類")]
        [Display(Name = "產品分類")]
        public int CategoryId { get; set; }

        [StringLength(2000, ErrorMessage = "描述最多2000字")]
        [Display(Name = "產品描述")]
        public string Description { get; set; }

        [Range(0, 120, ErrorMessage = "保固月份範圍 0-120")]
        [Display(Name = "保固月份")]
        public int? WarrantyMonth { get; set; }

        [Display(Name = "需要組裝")]
        public bool AssemblyRequired { get; set; }

        [StringLength(255, ErrorMessage = "組裝部件最多255字")]
        [Display(Name = "組裝部件")]
        public string AssemblyPart { get; set; }

        [Range(0, 100, ErrorMessage = "折扣範圍 0-100")]
        [Display(Name = "折扣")]
        public int? Discount { get; set; }

        [Required(ErrorMessage = "請選擇產品狀態")]
        [Display(Name = "產品狀態")]
        public int PStatusId { get; set; } = 1; // 預設上架中

        // 變體資訊
        public List<CProductVariantDTO> Variants { get; set; } = new List<CProductVariantDTO>();

        // 產品圖片
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
