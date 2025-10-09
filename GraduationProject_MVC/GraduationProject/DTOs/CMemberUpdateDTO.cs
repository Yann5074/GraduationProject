namespace GraduationProject.DTOs
{
    public class CMemberUpdateDTO
    {
        public bool isValid { get; set; }

        public int MemberId { get; set; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public int? Gender { get; set; }

        public string? Phone { get; set; }

        public DateOnly? BirthDate { get; set; }

        public string? Address { get; set; }

        public string? MemberImage { get; set; }

        public int? LeveId { get; set; }

        public int? MoneySum { get; set; }

        public int? Status { get; set; }

        public DateTime? CreatTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public string? Account { get; set; }

        public string? Passwords { get; set; }

        public IFormFile photo { get; set; }

        // 關聯名稱（選填）
        public string? GenderName { get; set; }
        public string? StatusName { get; set; }
        public string? LevelName { get; set; }
    }
}
