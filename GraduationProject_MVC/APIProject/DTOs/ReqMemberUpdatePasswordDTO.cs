namespace ApiProject.DTOs
{
    public class ReqMemberUpdatePasswordDTO
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
    }
}
