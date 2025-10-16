namespace ApiProject.DTOs
{
    public class ResApiResponseDTO<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
