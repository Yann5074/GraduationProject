namespace ApiProject.DTOs
{
    public class ResMemberDTO
    {
        public int MemberId { get; set; }
        public string Account { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public int? Gender { get; set; }
        public string? GenderName { get; set; }            // 👈 額外帶名稱
        public DateOnly? BirthDate { get; set; }

        public string Email {  get; set; }
        public string MemberImage { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }
        public string? AvatarUrl { get; set; }
        public int? LevelId { get; set; }
        public string? LevelName { get; set; }             // 👈 額外帶名稱
        public int? MoneySum { get; set; }
        public int? Status { get; set; }
        public string? StatusName { get; set; }            // 👈 額外帶名稱
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
