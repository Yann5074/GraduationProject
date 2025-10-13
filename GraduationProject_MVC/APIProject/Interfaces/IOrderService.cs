using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IOrderService
    {
        public Task<List<ResOrderDTO>> GetAllOrdersAsync();

        public Task<List<ResOrderDTO>> GetOrdersByIdAndProdNameAsync(string? keyword);



    }
}
