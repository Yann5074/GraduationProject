using GraduationProject.Interfaces;
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
        public IActionResult List(COrderSearchKeywordViewModel vm)
        {
            var query = _orderService.SearchOrder(vm);
            return View(query);
        }
    }
}
