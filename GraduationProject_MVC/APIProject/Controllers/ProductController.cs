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
        public async Task<IActionResult> GetProducts([FromQuery] ReqProductFilterDTO filter)
        {
            try
            {
                var result = await _ProductService.GetAllProductsAsync(filter);

                // ✅ 直接回傳商品列表
                return Ok(new ResApiResponseDTO<List<ResProductListDTO>>
                {
                    Success = true,
                    Message = "取得商品列表成功",
                    Data = result  // 直接是 List<ProductListDto>
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"取得商品列表失敗: {ex.Message}",
                    Data = null
                });
            }
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


        //[HttpDelete("{id:int}")]
        //public async Task<IActionResult> SoftDelete(int id)
        //{
        //    var ok = await _ProductService.SoftDeleteAsync(id);
        //    return ok ? NoContent() : NotFound();
        //}


    }
}
