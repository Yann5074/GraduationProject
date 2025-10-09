using Microsoft.AspNetCore.Mvc.Rendering;

namespace GraduationProject.ViewModels
{
    public class CProductVariantEditItem
    {
        public int ProductVariantId { get; set; }   // PK
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }          // fCost
        public int? Stock { get; set; }             // fStock
        public decimal? Length { get; set; }        // fLength
        public decimal? Width { get; set; }         // fWidth
        public decimal? Height { get; set; }        // fHeight
        public decimal? Weight { get; set; }        // fWeight
        public int? PStatusId { get; set; }         //變體狀態（fPStatus in Variant）
        public IEnumerable<SelectListItem> PStatusOptions { get; set; } = Enumerable.Empty<SelectListItem>(); // 若每筆要不同選單
        public string ColorName { get; set; }       // 顯示用（可選）
    }
}
