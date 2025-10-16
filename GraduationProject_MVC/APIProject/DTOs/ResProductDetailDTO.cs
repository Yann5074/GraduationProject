namespace ApiProject.DTOs
{
    public class ResProductDetailDTO: ResProductListDTO
    {
        public string FAssemblyPart { get; set; }
        public List<ResProductVariantDTO> Variants { get; set; }
        public List<ResProductAssetDTO> Assets { get; set; }
    }
}
