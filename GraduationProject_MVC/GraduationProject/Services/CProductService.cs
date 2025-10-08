using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.Intrinsics.X86;


namespace GraduationProject.Services
{
    public class CProductService : IProductService
    {
        private readonly dbFurniMartContext _db;
        public CProductService(dbFurniMartContext db) => _db = db;

        public IEnumerable<CProductDTO> SearchProduct(CProductSearchKeywordViewModel vm)
        {
            string keywordProductId = vm.txtKeywordProductId;
            string keywordMemberName = vm.txtKeywordProductName;
            string keywordMemberPhone = vm.txtKeywordCategoryId;
            var query = _db.TProducts
                .Include(o => o.ProductVariant)
                .Include(o => o.ProductAsset)
                .Include(o => o.Category)
                .Select(o => new CProductDTO
                {
                    ProductId = o.FProductId,
                    Name = o.FName,
                    Description = o.FDescription,
                    CategoryId = o.FCategoryId,
                    CategoryName = o.Category.FName,
                    PStatus = o.FPstatus,
                    Price = o.ProductVariant.FPrice,

                });
            return query.ToList();
        }


        public int CreateProduct(CProductCreateDTO dtoUi)
        {
            var od = new TProduct
            {
                FName = dtoUi.Name,
                FDescription = dtoUi.Description,
                FCategoryId = dtoUi.CategoryId,
                FPstatus = dtoUi.PStatus,
            
            };
            _db.TProducts.Add(od);
            _db.SaveChanges();
            return od.FProductId;
        }

    }
}
