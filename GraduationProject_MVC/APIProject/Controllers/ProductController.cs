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
                return Ok(new ResApiResponseDTO<ResultPagedDTO<ResProductListDTO>>
                {
                    Success = true,
                    Message = "取得商品列表成功",
                    Data = result
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

        [HttpGet("filter-options")]
        public async Task<IActionResult> GetFilterOptions()
        {
            try
            {
                var result = await _ProductService.GetFilterOptionsAsync();
                return Ok(new ResApiResponseDTO<ResFilterOptionsDTO>
                {
                    Success = true,
                    Message = "取得篩選選項成功",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"取得篩選選項失敗: {ex.Message}",
                    Data = null
                });
            }
        }

        //Get:api/product/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var result = await _ProductService.GetProductByIdAsync(id);

                if (result == null)
                {
                    return NotFound(new ResApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "找不到該商品",
                        Data = null
                    });
                }

                return Ok(new ResApiResponseDTO<ResProductDetailDTO>
                {
                    Success = true,
                    Message = "取得商品成功",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"取得商品失敗: {ex.Message}",
                    Data = null
                });
            }
        }

        //GET:api/product/search/茶几
        [HttpGet("search/{keyword}")]
        public async Task<IActionResult> SearchProductsByKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new ResApiResponseDTO<object>
                {
                    Success = false,
                    Message = "請提供搜尋關鍵字",
                    Data = null
                });
            }

            var result = await _ProductService.GetProductByProdNameAsync(keyword);

            return Ok(new ResApiResponseDTO<List<ResProductDTO>>
            {
                Success = true,
                Message = "搜尋成功",
                Data = result ?? new List<ResProductDTO>()
            });
        }


        [HttpGet("{id:int}/similar")]
        public async Task<IActionResult> GetSimilarProducts(int id,[FromQuery] int count = 4)
        {
            try
            {
                // 限制數量範圍
                if (count < 1) count = 4;
                if (count > 20) count = 20;

                var result = await _ProductService.GetSimilarProductsAsync(id, count);

                return Ok(new ResApiResponseDTO<List<ResProductListDTO>>
                {
                    Success = true,
                    Message = "取得相似商品成功",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"取得相似商品失敗: {ex.Message}",
                    Data = null
                });
            }
        }


        [HttpGet("{id:int}/customization")]
        public async Task<IActionResult> GetProductCustomization(int id)
        {
            try
            {
                var result = await _ProductService.GetProductCustomizationAsync(id);

                if (result == null)
                {
                    return NotFound(new ResApiResponseDTO<object>
                    {
                        Success = false,
                        Message = $"找不到產品 ID: {id} 或產品已下架",
                        Data = null
                    });
                }

                return Ok(new ResApiResponseDTO<ResProductCustomizationDTO>
                {
                    Success = true,
                    Message = "取得產品自訂資訊成功",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"伺服器錯誤: {ex.Message}",
                    Data = null
                });
            }
        }

    }
}
