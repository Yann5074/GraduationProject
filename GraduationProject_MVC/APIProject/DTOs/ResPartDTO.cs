namespace ApiProject.DTOs
{
    public class ResPartDTO
    {
        public int FPartId { get; set; }
        public string? FPartName { get; set; }      // "座墊"
        public string? FPartCode { get; set; }      // "seat_cushion"
        public int? fDisplayOrder { get; set; }
        public List<ResColorOptionDTO> ColorOptions { get; set; }
    }
}
