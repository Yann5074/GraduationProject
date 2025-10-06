using GraduationProject.Interfaces;
using GraduationProject.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers.Api
{
    [ApiController]                         //宣告這是 Web API 控制器
    [Route("api/[controller]")]              //路徑會變成 /api/Products
    public class ApiProductsController : ControllerBase  //用 ControllerBase，不繼承 Controller
    {
        private readonly IProductService _svc;

        public ApiProductsController(IProductService svc)
        {
            _svc = svc;
        }

        // 這支給 DataTables 呼叫
        [HttpPost("DataTable")]
        public async Task<IActionResult> DataTable()
        {
            // 從 DataTables 前端取得查詢參數
            var form = Request.Form;
            string? keyword = form["search[value]"];
            int start = int.TryParse(form["start"], out var s) ? s : 0;
            int length = int.TryParse(form["length"], out var l) ? l : 10;

            // 呼叫 Service 查詢資料
            var result = await _svc.DataTableAsync(new ProductDataTableQuery
            {
                Search = keyword,
                Start = start,
                Length = length
            });

            // 回傳 DataTables 規格格式
            return Ok(new
            {
                draw = form["draw"],
                recordsTotal = result.TotalRecords,
                recordsFiltered = result.FilteredRecords,
                data = result.Items
            });
        }
    }
}
