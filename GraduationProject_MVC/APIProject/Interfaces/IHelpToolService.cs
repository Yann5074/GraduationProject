using ApiProject.DTOs;
using ApiProject.Models;
using System.Security.Claims;

namespace ApiProject.Interfaces
{
    public interface IHelpToolService
    {
        public Task<CheckDTO> ValidateAndGetMemberAsync(ClaimsPrincipal user, CancellationToken cty);
    }
}
