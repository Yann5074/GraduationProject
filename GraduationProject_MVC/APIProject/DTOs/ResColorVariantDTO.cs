namespace ApiProject.DTOs
{
    public class ResColorVariantDTO
    {
        /// 變體 ID
        public int VariantId { get; set; }


        /// 顏色 ID
        public int ColorId { get; set; }


        /// 顏色名稱
        public string ColorName { get; set; }


        /// 顏色代碼
        public string ColorCode { get; set; }


        /// 顏色十六進位碼 (#FFFFFF) - 用於 Three.js
        public string ColorHex { get; set; }


        /// 顏色縮圖 URL
        public string? Thumbnail { get; set; }


        /// 價格
        public decimal Price { get; set; }


        /// 庫存
        public int Stock { get; set; }


        /// SKU
        public string SKU { get; set; }


        /// 尺寸標籤
        public string? SizeLabel { get; set; }
    }
}
