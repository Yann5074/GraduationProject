using GraduationProject.DTOs;

namespace GraduationProject.ViewModels
{
    public class CLeaveRequestItemViewModel
    {
        public IEnumerable<CLeaveItemDTO> LeaveRequests { get; set; }
        public int TotalRequests { get; set; }
    }
}
