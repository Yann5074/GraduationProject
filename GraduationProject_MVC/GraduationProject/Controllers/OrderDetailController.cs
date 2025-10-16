using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.Services;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class OrderDetailController : SuperController
    {
        private readonly IOrderDetailService _orderDetailService;
        public OrderDetailController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }
        public IActionResult List(int? id)
        {
            var query = _orderDetailService.ShowOrderDetail(id);
            ViewBag.OrderId = id;
            return View(query);
        }

        public IActionResult Delete(int? proId, int? orderId)
        {
            var result = _orderDetailService.DeleteOrderDetail(orderId, proId);
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

        //[HttpGet]
        public IActionResult Create(int? orderId)
        {
            //如果為空，則建立虛假的訂單明細，以顯示於畫面
            if (orderId == null)
                return RedirectToAction("List", "Order");
            var vm = new COrderDetailCreateViewModel
            {
                OrderId = (int)orderId,
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Create(COrderDetailCreateViewModel vm)
        {
            var dtoUi = new OrderDetailCreateDTO
            {
                OrderId = vm.OrderId,
                ProductVariantId = vm.ProductVariantId,
                Quantity = vm.Quantity,
            };
            var result = _orderDetailService.CreateOrderDetail(dtoUi);
            if (result == false)
            {
                TempData["createErrorMessage"] = "商品加入失敗，請重新操作";
                return RedirectToAction("List", new { id = vm.OrderId });
            }
            TempData["createSuccessMessage"] = "商品加入成功";
            return RedirectToAction("List", new {id = vm.OrderId});
        }
    }
}
