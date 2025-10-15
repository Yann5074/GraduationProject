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
        public async Task<List<ResProductDTO>> GetAllProducts()
        {
            var product = await _ProductService.AllProductAsync();
            return product;
        }





    }
}
