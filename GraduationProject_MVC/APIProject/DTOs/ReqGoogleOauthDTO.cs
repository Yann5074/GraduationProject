namespace ApiProject.DTOs
{
    // 前端送來的 payload：只要 idToken
    public class ReqGoogleOauthDTO
    {
        public string IdToken { get; set; } = "";
    }
    // 回應前端用的會員資料（對齊你 /login、/me 回傳欄位）
    public class MemberMeDTO
    {
        public int MemberId { get; set; }
        public string Account { get; set; } = "";
        public string? DisplayName { get; set; }
        public string Name { get; set; } = "";
        public string? GenderName { get; set; }
        public string? BirthDate { get; set; }
        public string Email { get; set; } = "";
        public string? MemberImage { get; set; }   // 檔名
        public string? AvatarUrl { get; set; }     // 可空，由前端 toImageUrl() 組也行
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int LevelId { get; set; }
        public string? LevelName { get; set; }
        public int MoneySum { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }

}
