namespace GraduationProject.DTOs
{
    public class CProductPartEditDTO
    {
        public int? PartId { get; set; }
        public string PartName { get; set; }
        public string PartCode { get; set; }
        public int? DisplayOrder { get; set; }
        public List<CProductColorOptionEditDTO> ColorOptions { get; set; } = new List<CProductColorOptionEditDTO>();
    }
}
