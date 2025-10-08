using GraduationProject.DTOs;
using GraduationProject.ViewModels;


namespace GraduationProject.Interfaces
{
    public interface IProductService
    {
        IEnumerable<CProductDTO> SearchProduct(CProductSearchKeywordViewModel vm);

        public int CreateProduct(CProductCreateDTO dto);

        


    }

}
