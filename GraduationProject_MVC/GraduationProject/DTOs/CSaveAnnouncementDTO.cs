using System.ComponentModel.DataAnnotations;

namespace GraduationProject.DTOs
{
    // 建立／更新公告時送給 API 的 DTO
    public class CSaveAnnouncementDTO
    {
        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public DateTime StartAt { get; set; }  // UTC

        public DateTime? EndAt { get; set; }   // UTC or null

        public bool IsActive { get; set; } = true;

        public int Priority { get; set; } = 0;

        /// <summary>
        /// 更新時傳遞併發控制 RowVersion
        /// </summary>
        //public byte[]? RowVersion { get; set; }
    }
}
