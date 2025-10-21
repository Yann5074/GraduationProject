namespace ApiProject.DTOs
{
    
    

        /// 產品價格查詢結果 DTO
        public class ResProductPriceDTO
        {

            /// 產品變體 ID
            public int FProductVariantId { get; set; }


            /// SKU
            public string FSKU { get; set; }


            /// 價格
            public decimal FPrice { get; set; }


            /// 庫存
            public int FStock { get; set; }


            /// 是否有貨
            public bool IsAvailable { get; set; }


            /// 產品變體狀態（1=上架, 2=下架）
            public int? FVariantStatus { get; set; }
        }
    
}
