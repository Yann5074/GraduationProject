namespace ApiProject.DTOs
{
    public class ResRecommonedDTO
    {
        public int ProductId { get; set; }

        public int ProductVariantId { get; set; }

        public string Name { get; set; }

        public string Size { get; set; } = "";

        public string? ImageUrl { get; set; }
    }
}
