using ApiProject.DTOs;

namespace ApiProject.Interfaces
{
    public interface IOrderService
    {
        public Task<List<ResOrderDTO>> GetAllOrdersAsync();

        public Task<List<ResOrderDTO>> GetOrdersByIdAndProdNameAsync(string? keyword);

        public Task<ResultDTO> DeleteOrderAsync(int orderId);

        public Task<ResultDTO> EditDeliveryAddressAsync(int orderId, ReqDeliveryAddressDTO reqDTO);

        public Task<ResultDTO> EditTaxNoAsync(int orderId, ReqTaxNoDTO reqDTO);

        public Task<ResultDTO> CreateOrderFromCartAsync(int memberId, ReqCreateOrderDTO dto);

        public Task<ResultDTO> CreateOrderFromGuestAsync(ReqGuestOrderDTO reqDto);

    }
}
