using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class CProductEditViewModel
    {
        // 商品本體
        public int ProductId { get; set; }
        public string Name { get; set; }          // 原有
        public string Description { get; set; }   // 原有
        public int? CategoryId { get; set; }      // 原有
        public int? PStatusId { get; set; }       // 原有（商品狀態）

        //新增：保固 / 組裝
        public int? WarrantyMonth { get; set; }        // fWarrantyMonth
        public bool? AssemblyRequired { get; set; }    // fAssemblyRequired
        public string AssemblyPart { get; set; }       // fAssemblyPart

        // 下拉
        public IEnumerable<SelectListItem> CategoryOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> PStatusOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        //變體（可多筆）
        public List<CProductVariantEditItem> Variants { get; set; } = new();

        //圖片資產（現有）
        public List<CProductAssetEditItem> Assets { get; set; } = new();

        //新增上傳（多檔）
        public List<IFormFile> NewPictures { get; set; } = new(); // 對應 <input type="file" multiple>
    }
}
