namespace ApiProject.DTOs
{

    // 庫存檢查結果 DTO
    public class ResStockCheckDTO
    {
        // 是否所有商品都有足夠庫存
        public bool IsAvailable { get; set; }


        // 庫存警告列表（庫存不足的商品）
        public List<StockWarningDTO> Warnings { get; set; }

        public ResStockCheckDTO()
        {
            Warnings = new List<StockWarningDTO>();
        }
    }


    // 庫存警告 DTO
    public class StockWarningDTO
    {
        // 產品變體 ID
        public int ProductVariantId { get; set; }


        // 產品名稱
        public string ProductName { get; set; }


        // SKU
        public string SKU { get; set; }


        // 需要的數量
        public int RequestedQuantity { get; set; }


        // 可用庫存
        public int AvailableStock { get; set; }


        // 警告訊息
        public string Message { get; set; }


        /// 警告類型：out_of_stock（缺貨）, insufficient_stock（庫存不足）, unavailable（已下架）
        public string WarningType { get; set; }
    }
}
