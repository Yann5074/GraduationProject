using ApiProject.DTOs;
using System.Security.Claims;

namespace ApiProject.Interfaces
{
    public interface IOrderService
    {
        public Task<List<ResOrderDTO>> GetAllOrdersAsync(ClaimsPrincipal user, CancellationToken ct);

        public Task<List<ResOrderDTO>> GetOrdersByIdAndProdNameAsync(string? keyword, ClaimsPrincipal user, CancellationToken ct);

        public Task<ResultDTO> DeleteOrderAsync(int orderId, ClaimsPrincipal user, CancellationToken ct);

        public Task<ResultDTO> EditDeliveryAddressAsync(int orderId, ReqDeliveryAddressDTO reqDTO, ClaimsPrincipal user, CancellationToken ct);

        public Task<ResultDTO> EditTaxNoAsync(int orderId, ReqTaxNoDTO reqDTO, ClaimsPrincipal user, CancellationToken ct);

        public Task<ResultDTO> CreateOrderFromCartAsync(ReqCreateOrderDTO dto, ClaimsPrincipal user, CancellationToken ct);

        public Task<ResultDTO> CreateOrderFromGuestAsync(ReqGuestOrderDTO reqDto);


    }
}
