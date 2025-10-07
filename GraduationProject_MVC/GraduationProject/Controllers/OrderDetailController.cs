using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.Services;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class OrderDetailController : Controller
    {
        private readonly IOrderDetailService _orderDetailService;
        public OrderDetailController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }
        public IActionResult List(int? id)
        {
            var query = _orderDetailService.ShowOrderDetail(id);
            return View(query);
        }

        public IActionResult Delete(int? proId, int? orderId)
        {
            var result = _orderDetailService.DeleteOrderDetail(proId);
            if (result == false)
            {
                TempData["deleteErrorMessage"] = "品項刪除失敗，請重新操作";
                return RedirectToAction("List", "OrderDetail", new {id = orderId});
            }
            TempData["deleteSuccessMessage"] = "品項刪除成功";
            return RedirectToAction("List", "OrderDetail", new {id = orderId});
        }

        public IActionResult Edit(int? proId, int? orderId)
        {
            var dto = _orderDetailService.SearchOrderDetail(proId);
            if (dto == null)
            {
                TempData["searchErrorMessage"] = "查無此商品明細，請重新操作";
                return RedirectToAction("List", "OrderDetail", new {id = orderId});
            }

            //轉DTO 到 VM 已顯示
            var vm = new COrderDetailViewModel
            {
                OrderId = dto.OrderId,
                ProductVariantId = dto.ProductVariantId,
                ProductName = dto.ProductName,
                IsDeleted = dto.IsDeleted,
                UnitPrice = dto.UnitPrice,
                Quantity = dto.Quantity,
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Edit(COrderDetailViewModel vm, int? orderId)
        {
            var dtoUi = new OrderDetailUpdateDTO
            {
                OrderId = vm.OrderId,
                ProductVariantId = vm.ProductVariantId,
                Quantity = vm.Quantity,
            };
            var result = _orderDetailService.UpdateOrderDetail(dtoUi);
            if (result == false)
            {
                TempData["updateErrorMessage"] = "更新商品明細失敗";
                return RedirectToAction("List", "OrderDetail", new { id = orderId });
            }

            TempData["updateSuccessMessage"] = "成功更新商品明細";
            return RedirectToAction("List", "OrderDetail", new { id = orderId });
        }
    }
}
