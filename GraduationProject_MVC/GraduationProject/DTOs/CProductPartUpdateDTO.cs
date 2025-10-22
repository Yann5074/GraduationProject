namespace GraduationProject.DTOs
{
    public class CProductPartUpdateDTO
    {
        public int? PartId { get; set; }   // null = 新增
        public bool? Deleted { get; set; }
        public string PartName { get; set; } = default!;
        public string? PartCode { get; set; }
        public int? DisplayOrder { get; set; }
        public List<CPartColorOptionUpdateDTO> Options { get; set; } = new();
    }
}
