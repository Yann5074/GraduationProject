using ApiProject.DTOs;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Security.Claims;

namespace ApiProject.Interfaces
{
    public interface ICartService
    {
        public Task<List<ResCartDTO>> GetAllCartAsync(ClaimsPrincipal user, CancellationToken ct);

        public Task<ResultDTO> DeleteCartItemAsync(int cartItemId);

        public Task<ResultDTO> DeleteCartAsync(int cartId);

        public Task<ResultDTO> EditCartItemNumAsync(int cartItemId, ReqEditCartItemNumDTO reqDto);

        public Task<ResultDTO> CreateCartAsync(ReqCartDTO reqDto, ClaimsPrincipal user, CancellationToken ct);

        public Task<ResultDTO> ValidateCartAsync(ClaimsPrincipal user, CancellationToken ct);

        public Task<ResultDTO> SyncCartAsync(ReqSyncCartDTO reqDto, ClaimsPrincipal user, CancellationToken ct);
    }
}
