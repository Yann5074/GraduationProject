using System.ComponentModel.DataAnnotations;

namespace GraduationProject.ViewModels
{
    public class CLeaveCreateViewModel
    {
        [Required]
        public string LeaveType { get; set; } = "";
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }

        // 上傳證明
        public IFormFile? Picture { get; set; }
    }
}
