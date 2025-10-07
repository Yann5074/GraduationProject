using GraduationProject.DTOs;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Interfaces
{
    public interface IOrderService
    {
        //Search 搜尋訂單
        IEnumerable<OrderSearchDTO> SearchOrder(COrderSearchKeywordViewModel vm);

        //Create 建立訂單
        public bool CreateOrder(OrderCreateDTO dto);

        //Update 更新訂單
        public bool UpdateOrder(OrderUpdateDTO dto);

        //Delete 刪除訂單
        public bool DeleteOrder(int? id);

        //SearchUpdate 搜尋欲更新訂單
        public OrderUpdateDTO SearchUpdateOrder(int? id);
    }
}
