using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class CAnnouncementViewModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "標題")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "內容")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [Display(Name = "開始時間")]
        public DateTime StartAtLocal { get; set; } = DateTime.Now;

        [Display(Name = "結束時間")]
        public DateTime? EndAtLocal { get; set; }

        [Display(Name = "啟用")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "優先順序（越大越前）")]
        public int Priority { get; set; } = 0;

        // 併發控制（若 API 有啟用 rowversion）
        //public byte[]? RowVersion { get; set; }
    }
}
