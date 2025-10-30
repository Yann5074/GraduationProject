namespace ApiProject.DTOs
{
    public class ReqResetPasswordDTO
    {
        public string Account { get; set; } = "";
        public string Email { get; set; } = "";
        public string Code { get; set; } = "";
        public string NewPassword { get; set; } = "";
        public string? ConfirmNewPassword { get; set; }
    }
}
