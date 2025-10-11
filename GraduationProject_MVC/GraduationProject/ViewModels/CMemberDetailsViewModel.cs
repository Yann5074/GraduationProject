namespace GraduationProject.ViewModels
{
    public class CMemberDetailsViewModel
    {
        public int MemberId { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? GenderName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? StatusName { get; set; }
        public string? LevelName { get; set; }
        public string? MemberImage { get; set; }
        public int? MoneySum { get; set; }
        public DateTime? CreatTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
