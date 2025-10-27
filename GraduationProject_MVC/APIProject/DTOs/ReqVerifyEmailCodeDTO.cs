namespace ApiProject.DTOs
{
    public class ReqVerifyEmailCodeDTO
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}
