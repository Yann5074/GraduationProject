using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IOrderService
    {
        public Task<List<ResOrderDTO>> GetAllOrdersAsync();

        public Task<List<ResOrderDTO>> GetOrdersByIdAndProdNameAsync(string? keyword);

        public Task<ResultDTO> DeleteOrderAsync(int orderId);

        public Task<ResultDTO> EditDeliveryAddressAsync(int orderId, ReqDeliveryAddressDTO reqDTO);

    }
}
