namespace ApiProject.DTOs
{
    public class ResCartDTO
    {
        public int MemberId { get; set; }
        public int CartId { get; set; }

        public decimal TotalPrice { get; set; }

        public IEnumerable<ResCartItemDTO> CartItem { get; set; } = new List<ResCartItemDTO>();

    }
}