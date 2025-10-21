namespace ApiProject.DTOs
{
    // 用於回傳呼叫結果
    public class ResultDTO
    {
        public bool Ok { get; set; }
        public int Code { get; set; }
        public object? Message { get; set; }

    }
}