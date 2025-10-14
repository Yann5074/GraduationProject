using System.ComponentModel.DataAnnotations;

namespace ApiProject.DTOs
{
    public class ResOrderDTO
    {
        public string OrderId { get; set; }
        public int IsDeleted { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string OrderTime { get; set; }
        public int OrderStatusId { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalPrice { get; set; }

        public IEnumerable<ResOrderDetailDTO> OrderDetail { get; set; } = new List<ResOrderDetailDTO>();

    }
}