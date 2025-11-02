namespace ApiProject.DTOs
{
    public class PBRMaterialParams
    {

        /// 金屬度值 (0.0 - 1.0)
        public float Metalness { get; set; } = 0.0f;


        /// 粗糙度值 (0.0 - 1.0)
        public float Roughness { get; set; } = 0.5f;


        /// 環境光強度

        public float EnvMapIntensity { get; set; } = 1.0f;

        /// 法線貼圖強度
        public float NormalScale { get; set; } = 1.0f;


        /// AO 強度
        public float AOMapIntensity { get; set; } = 1.0f;


        /// 自發光強度
        public float EmissiveIntensity { get; set; } = 1.0f;
    }
}
