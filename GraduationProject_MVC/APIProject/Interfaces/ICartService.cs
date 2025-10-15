using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface ICartService
    {
        public Task<ResCartDTO> GetAllCartAsync();
    }
}
