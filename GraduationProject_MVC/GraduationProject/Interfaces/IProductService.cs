using GraduationProject.DTOs;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace GraduationProject.Interfaces
{
    public interface IProductService
    {
        IEnumerable<CProductDTO> SearchProduct(CProductSearchKeywordViewModel vm);
        CProductDetailDTO? GetDetail(int productId); // 給 Edit GET 使用（你可自定）
        int Update(CProductUpdateDTO dto);
        bool Delete(int productId);

        (int ProductId, Dictionary<int, int> VariantIdMap) CreateProductAndVariants(CProductUpdateDTO dto);

        void CreateAssetsForProduct(int productId, List<CProductAssetDTO> assets);
        int Create(CProductUpdateDTO dto);

        bool SkuExists(string sku, int? excludeVariantId = null);

        (string url, string? mime) UploadAsset(IFormFile file);

        // ===== 下拉選單（分類 / 顏色 / 狀態 / 貼圖） =====
        List<SelectListItem> GetCategoryOptions();
        List<SelectListItem> GetColorOptions();
        List<SelectListItem> GetPStatusOptions();
        List<SelectListItem> GetTextureOptions();
    }

}
