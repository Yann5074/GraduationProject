namespace ApiProject.DTOs
{
    //購物車商品資訊
    public class ResCartProductDTO
    {

        /// 產品變體 ID
        public int FProductVariantId { get; set; }

        /// 產品 ID
        public int FProductId { get; set; }


        /// 產品名稱
        public string FProductName { get; set; }


        /// SKU
        public string FSKU { get; set; }


        /// 價格
        public decimal FPrice { get; set; }


        /// 庫存
        public int FStock { get; set; }


        /// 是否有貨
        public bool IsAvailable { get; set; }


        /// 主要圖片 URL
        public string MainImageUrl { get; set; }


        /// 產品狀態（1=上架, 0=下架）
        public int? FProductStatus { get; set; }


        /// 變體狀態（1=上架, 0=下架）
        public int? FVariantStatus { get; set; }


        /// 是否為可自訂產品
        public bool IsCustomizable { get; set; }


        /// 顏色組合描述（用於顯示）
        /// 例如："淺橡木桌面 + 白色烤漆腳"
        public string ColorCombinationDescription { get; set; }


        /// 顏色組合詳細資訊
        /// 例如：[ { "partName": "桌面", "optionName": "淺橡木色" } ]
        public List<ResCartColorOptionDTO> CartColorOptions { get; set; }
    }
}
