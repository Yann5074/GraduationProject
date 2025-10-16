namespace GraduationProject.DTOs
{
    public class CAuthResultDTO
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public SessionUser? User { get; init; }
    }
    public class SessionUser
    {
        public int Id { get; init; }
        public string? Name { get; init; }
        public string? HeadShot { get; init; }
        public string Account { get; init; } = default!;
        public string? Email { get; init; }
        public int? RoleId { get; init; }
        public int? StatusId { get; init; }
    }
}
