using GraduationProject.Interfaces;
using GraduationProject.Services;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _anylyticsService;
        public AnalyticsController(IAnalyticsService anylyticsService)
        {
            _anylyticsService = anylyticsService;
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
