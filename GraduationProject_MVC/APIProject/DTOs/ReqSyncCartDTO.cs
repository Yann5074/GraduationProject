using ApiProject.Models;

namespace ApiProject.DTOs
{
    public class ReqSyncCartDTO
    {
        public int MemberId { get; set; }

        public ICollection<ReqCartItemDTO> CartItem  { get; set; }
    }
}
