using GraduationProject.Interfaces;
using GraduationProject.Services;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

namespace GraduationProject.Controllers
{
    public class AnalyticsController : SuperController
    {
        private readonly IAnalyticsService _anylyticsService;
        public AnalyticsController(IAnalyticsService anylyticsService)
        {
            _anylyticsService = anylyticsService;
        }

        //BestVariantsDashboard
        public async Task<IActionResult> BestVariantsDashboard(DateTime? start, DateTime? end, CancellationToken ct)
        {
            var dto = await _anylyticsService.GetBestSellingVariantsAsync(top: 10, start, end, onlyCompletedOrders: true, ct);
            var top = dto.FirstOrDefault();
            if (top == null) return View(new CBestSellerItemViewModel());

            var list = dto.Select(x => new CBestSellerItemViewModel
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                VariantId = x.VariantId,
                SKU = x.SKU,
                TotalQty = x.TotalQty,
                TotalRevenue = x.TotalRevenue
            }).ToList();
            return View(list);
        }

        //MemberDashboard
        public async Task<IActionResult> MemberDashboard(
        DateTime? start, DateTime? end, CancellationToken ct)
        {
            // 顯示最近 6 個月
            if (!start.HasValue || !end.HasValue)
            {
                var e = DateTime.Today;
                var s = new DateTime(e.Year, e.Month, 1).AddMonths(-5);
                start = s; end = e;
            }

            var dto = await _anylyticsService.GetMemberDashboardAsync(start, end, ct);

            var vm = new CMemberGrowthViewModel
            {
                Labels = dto.Labels,
                NewMembers = dto.NewMembers,
                GrowthRates = dto.GrowthRates,
                TotalMembers = dto.TotalMembers,
                ActiveMembers = dto.ActiveMembers
            };

            return View(vm);
        }

        //OrderDashboard
        public async Task<IActionResult> OrderDashboard(CancellationToken ct)
        {
            var dto = await _anylyticsService.GetDashboardAsync(ct);

            var vm = new COrderDashboardViewModel
            {
                MonthlyLabels = dto.MonthlyLabels,
                MonthlySales = dto.MonthlySales,
                StatusLabels = dto.StatusLabels,
                StatusCounts = dto.StatusCounts,
                PaymentLabels = dto.PaymentLabels,
                PaymentCounts = dto.PaymentCounts,

                MaxCompletedMonths = dto.MaxCompletedMonths
                .Select(x => new MonthCountItemVM { Label = x.Label, Count = x.Count }).ToList(),
                MinCompletedMonths = dto.MinCompletedMonths
                .Select(x => new MonthCountItemVM { Label = x.Label, Count = x.Count }).ToList(),
                MaxCanceledMonths = dto.MaxCanceledMonths
                .Select(x => new MonthCountItemVM { Label = x.Label, Count = x.Count }).ToList(),
                MinCanceledMonths = dto.MinCanceledMonths
                .Select(x => new MonthCountItemVM { Label = x.Label, Count = x.Count }).ToList()
            };

            return View(vm);
        }
        
    }
}
