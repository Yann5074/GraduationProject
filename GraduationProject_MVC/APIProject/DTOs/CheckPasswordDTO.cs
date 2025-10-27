namespace ApiProject.DTOs
{
    // 前端傳進來的資料
    public class CheckPasswordRequestDTO
    {
        public string? Password { get; set; }
    }

    // 回前端的資料
    public class CheckPasswordResponseDTO
    {
        public bool Ok { get; set; }
        public string? Message { get; set; }
    }
}
