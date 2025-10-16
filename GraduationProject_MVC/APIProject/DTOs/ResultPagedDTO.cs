namespace ApiProject.DTOs
{
    public class ResultPagedDTO<T>
    {
        public List<T> Data { get; set; }
        public ResPaginationDTO Pagination { get; set; }
    }
}
