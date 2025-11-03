using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GraduationProject.Controllers
{
    public class OrderController : SuperController
    {
        private readonly IOrderService _orderService;
        private readonly IAnalyticsService _anylyticsService;
        public OrderController(IOrderService orderService, IAnalyticsService anylyticsService)
        {
            _orderService = orderService;
            _anylyticsService = anylyticsService;
        }

        // Order/List
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
                TempData["DeleteErrorMessage"] = "刪除失敗，查無指定的訂單";
                return RedirectToAction("List");
            }
            TempData["DeleteSuccessMessage"] = "刪除訂單成功";
            return RedirectToAction("List");
        }

        // Order/Edit [HttpGet]
        public IActionResult Edit(int? id)
        {
            var dto = _orderService.SearchUpdateOrder(id);
            if (dto.isValid == false)
            {
                TempData["SearchUpdateErrorMessge"] = "查無指定的訂單";
                return RedirectToAction("List");
            }

            //將 DTO 轉換成 VM 以傳遞到 View 顯示
            var vm = new COrderEditViewModel
            {
                OrderId = dto.OrderId,
                EmployeeName = dto.EmployeeName,
                Discount = dto.Discount,
                OrderStatus = dto.OrderStatus,
                PaymentStatus = dto.PaymentStatus,
                PickupMethod = dto.PickupMethod,
                DeliveryStatus = dto.DeliveryStatus,
                DeliveryAddress = dto.DeliveryAddress,
                ShippingCost = dto.ShippingCost,
                DeliveryTime = dto.DeliveryTime,
                LogisticsProvider = dto.LogisticsProvider,
                OrderCompletionTime = dto.OrderCompletionTime,
                FNote = dto.FNote,
            };
            return View(vm);
        }

        // Order/Edit [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Edit(COrderEditViewModel vm)
        {
            // 將 VM 轉回 DTO 以傳遞至 Service 計算
            var dtoUi = new OrderUpdateDTO
            {
                OrderId = vm.OrderId,
                EmployeeName = vm.EmployeeName,
                Discount = vm.Discount,
                OrderStatus = vm.OrderStatus,
                PaymentStatus = vm.PaymentStatus,
                PickupMethod = vm.PickupMethod,
                DeliveryStatus = vm.DeliveryStatus,
                DeliveryAddress = vm.DeliveryAddress,
                ShippingCost = vm.ShippingCost,
                DeliveryTime = vm.DeliveryTime,
                LogisticsProvider = vm.LogisticsProvider,
                OrderCompletionTime = vm.OrderCompletionTime,
                FNote = vm.FNote,
            };

            var result = await _orderService.UpdateOrder(dtoUi);
            if (result == false)
            {
                TempData["UpdateErrorMessage"] = "更新失敗";
                return RedirectToAction("List");
            }

            TempData["UpdateSuccessMessage"] = "訂單更新成功";
            return RedirectToAction("List");
        }

        // Order/FindMember [HttpGet]
        public IActionResult FindMember()
        {
            return View();
        }

        // Order/FindMember [HttpPost]
        [HttpPost]
        public IActionResult FindMember(CMemberSearchViewModel vm)
        {
            // 呼叫 MemberServices 的查詢 TODO
            // 0912345678 - 10020 - 周杰倫
            // 呼叫要返還 ID 與 NAME
            return RedirectToAction("Create", new {memberId = 10004});
        }

        // Order/Create [HttpGet]
        public IActionResult Create(int? memberId)
        {
            if (memberId == null)
                return RedirectToAction("FindMember");
            var vm = new COrderCreateViewModel
            {
                OrderTime = DateTime.Now,
                MemberId = (int)memberId,
                MemberName = "10012",
                // 要在最一開始便判定登入的員工是誰 TODO
                // 20 - LinYan
                EmployeeId = 20,
                EmployeeName = "LinYan",
                // 折扣要透過 Membere關聯的資料帶入 TODO
                OrderStatus = 2,
                PaymentStatus = 1,

            };
            return View(vm);
        }
        [HttpPost]
        public IActionResult Create(COrderCreateViewModel vm)
        {
            if (vm == null)
            {
                TempData["createErrorMessage"] = "建立訂單失敗，請重新建立";
                return RedirectToAction("List");
            }

            // VM 轉 DTO for Service
            var dto = new OrderCreateDTO
            {
                MemberId = vm.MemberId,
                EmployeeId= vm.EmployeeId,
                TotalPrice = vm.TotalPrice,
                Discount = vm.Discount,
                TaxNo = vm.TaxNo,
                OrderTime = vm.OrderTime,
                OrderStatus= vm.OrderStatus,
                PaymentMethod = vm.PaymentMethod,
                PaymentStatus= vm.PaymentStatus,
                PaymentTime= vm.PaymentTime,
                PickupMethod = vm.PickupMethod,
                DeliveryStatus = vm.DeliveryStatus,
                DeliveryAddress = vm.DeliveryAddress,
                ShippingCost = vm.ShippingCost,
                DeliveryTime = vm.DeliveryTime,
                LogisticsProvider = vm.LogisticsProvider,
                OrderCompletionTime = vm.OrderCompletionTime,
                Note = vm.Note,
            };
            var result = _orderService.CreateOrder(dto);
            if (result)
                TempData["createSuccessMessage"] = "訂單建立成功";
            return RedirectToAction("List");
        }
    }
}
