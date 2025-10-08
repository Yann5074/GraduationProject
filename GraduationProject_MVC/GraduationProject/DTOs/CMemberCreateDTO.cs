namespace GraduationProject.DTOs
{
    public class CMemberCreateDTO
    {
        public string FName { get; set; } = default!;
        public string? FDisplayName { get; set; }
        public int? FGender { get; set; }
        public string? FPhone { get; set; }
        public DateTime? FBirthDate { get; set; }
        public string? FAddress { get; set; }

    }

    // 建立完成後回傳給 Controller 用（想顯示成功訊息會用到）
    public class CMemberCreatedDTO
    {
        public int FMemberId { get; set; }
        public string FName { get; set; } = default!;
        public string? FDisplayName { get; set; }
    }
}
