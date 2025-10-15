using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;

namespace ApiProject.Services
{
    public class CCartService : ICartService
    {
        private readonly dbFurniMartContext _context;
        public CCartService(dbFurniMartContext context)
        {
            _context = context;
        }

        public async Task<ResCartDTO> GetAllCartAsync()
        {
            //var cart = _context.TShoppingCarts

            return  new ResCartDTO
            {

            };
        }
    }
}
