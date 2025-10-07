using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class EmployeeController : Controller
    {
        //建構子示範直接把 DbContext 注入控制器
        //dbFurniMartContext _context;
        //public EmployeeController(dbFurniMartContext context)
        //{
        //    _context = context;
        //}

        //宣告一個私有唯讀欄位，型別是員工介面。
        private readonly IEmployeeService _svc;
        //相依性注入（DI）建構子：框架在建立控制器時，會把已註冊的 IEmployeeService 實例丟進來。
        public EmployeeController(IEmployeeService svc) => _svc = svc;

        //async Task<IActionResult>非同步寫法
        //CancellationToken ct：要求處理中可被取消（例如使用者關閉頁面）
        public async Task<IActionResult> List(CEmplKeywordViewModel vm, CancellationToken ct)
        {
            //呼叫 service 取得資料清單
            //把 vm.Keyword 傳進去當搜尋關鍵字，並把 ct 傳遞下去，整條查詢可被取消
            var items = await _svc.GetEmployeeListAsync(vm.Keyword, ct);
            return View(items); // View 的 model 改為 IEnumerable<CEmployeeListItemDTO>
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
