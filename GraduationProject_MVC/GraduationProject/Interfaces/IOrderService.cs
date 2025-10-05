using GraduationProject.DTOs;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Interfaces
{
    public interface IOrderService
    {
        //Search 搜尋訂單
        IEnumerable<OrderDTO> SearchOrder(COrderSearchKeywordViewModel vm);
        //Create 建立訂單
        public void CreateOrder();
        //Update 更新訂單
        public void UpdateOrder();

        //Delete 刪除訂單
        public void DeleteOrder();
    }
}
