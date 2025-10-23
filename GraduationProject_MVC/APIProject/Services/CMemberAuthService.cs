using ApiProject.DTOs;
using ApiProject.Interfaces;
using ApiProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using System.Security.Claims;

namespace ApiProject.Services
{
    public class CMemberAuthService : IHelpToolService
    {
        private readonly dbFurniMartContext _context;
        public CMemberAuthService(dbFurniMartContext context)
        {
            _context = context;
        }

        public async Task<CheckDTO> ValidateAndGetMemberAsync(ClaimsPrincipal user, CancellationToken cty)
        {
            var idCheck = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idCheck))
                return new CheckDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "未登入或登入逾期，請重新登入"
                };

            var memberId = int.Parse(idCheck);
            var member = await _context.TMembers.FirstOrDefaultAsync(m => m.FMemberId == memberId && m.FStatus == 1);

            if (member == null)
                return new CheckDTO
                {
                    Ok = false,
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "查無此會員或帳號無效，請洽客服"
                };

            return new CheckDTO
            {
                Ok = true,
                Code = StatusCodes.Status204NoContent,
                Member = member
            };
        }
                
    }
}
