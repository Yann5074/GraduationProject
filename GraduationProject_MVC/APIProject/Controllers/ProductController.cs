using ApiProject.DTOs;
using ApiProject.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("VueClient")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _ProductService;

        public ProductController(IProductService ProductService) => _ProductService = ProductService;

        // GET:api/products
        [HttpGet("all")]
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
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id, [FromQuery] bool includeCustomization = false)
        {
            try
            {
                var result = await _ProductService.GetProductByIdAsync(id);

                if (result == null)
                {
                    return NotFound(new ResApiResponseDTO<object>
                    {
                        Success = false,
                        Message = $"找不到產品 ID: {id}",
                        Data = null
                    });
                }

                return Ok(new ResApiResponseDTO<ResProductDetailDTO>
                {
                    Success = true,
                    Message = "取得產品詳情成功",
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


        //Get:api/product/1/similar
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






        // 取得單一產品變體（GET）
        // GET /api/front/product/cart-items/101
        [HttpGet("cart-items/{variantId:int}")]
        public async Task<IActionResult> GetCartProduct(int variantId)
        {
            var result = await _ProductService.GetCartProductsAsync(new List<int> { variantId });

            if (!result.Any())
            {
                return NotFound();
            }

            return Ok(new ResApiResponseDTO<ResCartProductDTO>
            {
                Success = true,
                Message = "取得商品資訊成功",
                Data = result[0]  // 回傳單一物件
            });
        }


        // 批次取得產品變體（POST）
        // POST /api/front/product/cart-items
        [HttpPost("cart-items")]
        public async Task<IActionResult> GetCartProducts([FromBody] List<int> productVariantIds)
        {
            var result = await _ProductService.GetCartProductsAsync(productVariantIds);

            return Ok(new ResApiResponseDTO<List<ResCartProductDTO>>
            {
                Success = true,
                Message = "取得商品資訊成功",
                Data = result  // 回傳列表
            });
        }

        //價格和庫存資訊
        // POST /api/front/product/{id}/price
        [HttpPost("{id:int}/price")]
        public async Task<IActionResult> GetPriceByCustomization(
           int id,
           [FromBody] Dictionary<string, int> selectedOptions)
        {
            try
            {
                // 驗證輸入
                if (selectedOptions == null || !selectedOptions.Any())
                {
                    return BadRequest(new ResApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "請提供顏色選項",
                        Data = null
                    });
                }

                var result = await _ProductService.GetPriceByCustomizationAsync(id, selectedOptions);

                if (result == null)
                {
                    return NotFound(new ResApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "找不到此顏色組合的產品變體",
                        Data = null
                    });
                }

                return Ok(new ResApiResponseDTO<ResProductPriceDTO>
                {
                    Success = true,
                    Message = "查詢價格成功",
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


        //批次檢查庫存
        //要檢查的商品列表
        //庫存檢查結果
        // POST /api/front/product/check-stock
        [HttpPost("check-stock")]
        public async Task<IActionResult> CheckStock([FromBody] List<ReqStockCheckItemDTO> items)
        {
            try
            {
                // 驗證輸入
                if (items == null || !items.Any())
                {
                    return BadRequest(new ResApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "請提供要檢查的商品列表",
                        Data = null
                    });
                }

                // 驗證每個項目
                foreach (var item in items)
                {
                    if (item.ProductVariantId <= 0)
                    {
                        return BadRequest(new ResApiResponseDTO<object>
                        {
                            Success = false,
                            Message = "產品變體 ID 必須大於 0",
                            Data = null
                        });
                    }

                    if (item.Quantity <= 0)
                    {
                        return BadRequest(new ResApiResponseDTO<object>
                        {
                            Success = false,
                            Message = "數量必須大於 0",
                            Data = null
                        });
                    }
                }

                var result = await _ProductService.CheckStockAsync(items);

                return Ok(new ResApiResponseDTO<ResStockCheckDTO>
                {
                    Success = true,
                    Message = result.IsAvailable ? "庫存充足" : "部分商品庫存不足",
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
