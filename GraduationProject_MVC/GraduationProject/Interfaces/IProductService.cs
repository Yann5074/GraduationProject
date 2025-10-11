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

        int Create(CProductCreateDto dto); // 同步回傳新商品ID
        bool Delete(int id); 
    }

}
