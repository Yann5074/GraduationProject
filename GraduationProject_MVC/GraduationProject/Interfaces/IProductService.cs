using GraduationProject.DTOs;
using GraduationProject.ViewModels;


namespace GraduationProject.Interfaces
{
    public interface IProductService
    {
        IEnumerable<CProductDTO> SearchProduct(CProductSearchKeywordViewModel vm);

        CProductDetailDTO GetProductDetail(int id); // 同步

        CProductEditViewModel GetProductForEdit(int id); // 讀取資料進編輯頁
        bool UpdateProduct(CProductEditViewModel vm);    // 寫回資料庫

        (bool Success, string Message, int? ProductId) CreateProduct(CProductCreateDTO dto);
        bool Delete(int id); 
    }

}
