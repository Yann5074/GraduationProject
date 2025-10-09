using GraduationProject.DTOs;

namespace GraduationProject.Interfaces
{
    public interface IOrderDetailService
    {
        // 列出所有訂單明細
        public IEnumerable<OrderDetailShowDTO> ShowOrderDetail(int? id);
        // 新增訂單明細
        bool CreateOrderDetail(OrderDetailCreateDTO dtoUi);
        // 修改訂單明細
        bool UpdateOrderDetail(OrderDetailUpdateDTO dtoUi);
        // 刪除訂單明細
        bool DeleteOrderDetail(int? id);
        // 搜尋欲修改訂單明細
        public OrderDetailUpdateDTO SearchOrderDetail(int? id);
    }
}
