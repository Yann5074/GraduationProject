namespace ApiProject.DTOs
{
    public class ResFilterOptionsDTO
    {
        public List<ResCategoryOptionDTO> Categories { get; set; }
        public ResPriceRangeDTO PriceRange { get; set; }
        public List<ResStatusOptionDTO> StatusOptions
        {
            get; set;
        }
    }
}
