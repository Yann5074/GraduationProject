using ApiProject.DTOs;
using ApiProject.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _ProductService;

        public ProductController(IProductService ProductService) => _ProductService = ProductService;

        // GET:api/products
        [HttpGet]
        public async Task<List<ResProductDTO>> GetAllProducts()
        {
            var product = await _ProductService.GetAllProductAsync();
            return product;
        }

        // GET:api/Product/keyword
        [HttpGet("{keyword}")]
        public async Task<List<ResProductDTO>> GetProductsByIdAndProdName(string? keyword)
        {
            if (keyword.IsNullOrEmpty())
                return null;
            var result = await _ProductService.GetProductByProdNameAsync(keyword);
            return result;
        }



    }
}
