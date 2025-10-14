using ApiProject.DTOs;
using ApiProject.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _ProductService;

        public ProductController(IProductService ProductService) => _ProductService = ProductService;

        // GET /api/products
        [HttpGet]
        public async Task<ActionResult<ResultPagedDTO<ResProductDTO>>> GetProducts([FromQuery] ReqProductQueryDTO query, CancellationToken ct)
        {
            var result = await _ProductService.GetProductsAsync(query, ct);
            return Ok(result);
        }

      



    }
}
