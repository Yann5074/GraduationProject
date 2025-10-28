using GraduationProject.DTOs;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace GraduationProject.Interfaces
{
    public interface IProductService
    {
        IEnumerable<CProductDTO> SearchProduct(CProductSearchKeywordViewModel vm);

        int Create(CProductCreateDTO dto);
        CProductDetailDTO? GetDetail(int productId); // 給 Edit GET 使用（你可自定）
        int Update(CProductUpdateDTO dto);


  

        bool Delete(int productId);

       
    }

}
