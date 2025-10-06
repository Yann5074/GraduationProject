using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        //Order/List
        public IActionResult List(COrderSearchKeywordViewModel vm)
        {
            var query = _orderService.SearchOrder(vm);
            return View(query);
        }

        // Order/Delete
        public IActionResult Delete(int? id)
        {
            var success = _orderService.DeleteOrder(id);
            if (!success)
            {
                TempData["ErrorMessage"] = "刪除失敗，查無指定的訂單";
                return RedirectToAction("List");
            }
            TempData["SuccessMessage"] = "刪除訂單成功";
            return RedirectToAction("List");
        }
    }
}
