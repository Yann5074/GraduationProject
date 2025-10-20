namespace ApiProject.DTOs
{
    public class ResProductCustomizationDTO
    {

        /// 產品 ID
        public int FProductId { get; set; }


        /// 產品名稱
        public string FProductName { get; set; }


        /// 產品描述
        public string FDescription { get; set; }


        /// 基礎價格（從第一個 ProductVariant 取得）
        public decimal BasePrice { get; set; }

        /// 分類 ID
        public int? FCategoryId { get; set; }


        /// 保固月數
        public int? FWarrantyMonth { get; set; }


        /// 是否需要組裝
        public bool? FAssemblyRequired { get; set; }


        /// 產品素材（圖片、影片、3D 模型）
        public List<ResProductAssetDTO> Assets { get; set; }


        /// 可自訂的部位
        public List<ResPartDTO> Parts { get; set; }
    }
}
