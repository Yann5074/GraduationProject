using System.ComponentModel.DataAnnotations;

namespace ApiProject.DTOs
{
    public class CSaveAnnouncementDTO
    {
        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        [Required]
        public DateTime StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public bool IsActive { get; set; } = true;

        public int Priority { get; set; } = 0;

        // 若未用 rowversion，可忽略
        //public byte[]? RowVersion { get; set; }
    }
}
