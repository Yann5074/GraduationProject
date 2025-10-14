namespace ApiProject.DTOs
{
    public class ReqMemberCreateDTO
    {
        public string Account { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public int? Gender { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
