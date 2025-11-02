namespace ApiProject.DTOs
{
    public class ResProductDetailDTO: ResProductListDTO
    {
        public int FProductId { get; set; }
        public string FName { get; set; }
        public string FDescription { get; set; }
        public int? FCategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? FWarrantyMonth { get; set; }
        public bool? FAssemblyRequired { get; set; }
        public int? FDiscount { get; set; }


        public string F3dModelPath { get; set; }        // 3D 模型檔案路徑
        public string EnvMapUrl { get; set; }


        //可用顏色選項（來自 tProductVariant + tColor）
        public List<ResColorVariantDTO> AvailableColors { get; set; } = new();

        // ===== 素材 =====
        public List<ResProductAssetDTO> Assets { get; set; }

        // ===== 變體和庫存 =====
        public List<ResProductVariantDTO> Variants { get; set; }
        public int TotalStock { get; set; }
        public bool IsAvailable { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // ===== 自訂資訊（可選） =====
        /// <summary>
        /// 是否為可自訂產品
        /// </summary>
        public bool IsCustomizable { get; set; }

        public List<ResProductImageDTO> Images { get; set; } = new();

        // ===== 時間 =====
        public DateTime? FCreateTime { get; set; }
        public DateTime? FUpdateTime { get; set; }
    }
}
