namespace ApiProject.DTOs
{
    public class ResTextureDTO
    {
        public int FTextureId { get; set; }
        public string? FTextureType { get; set; }   // "diffuse", "normal"
        public string? FFilePath { get; set; }
        public string? FTiling { get; set; }
    }
}
