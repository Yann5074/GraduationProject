namespace ApiProject.DTOs
{
    //購物車顏色選項
    public class ResCartColorOptionDTO
    {

        // 部位名稱：桌面、桌腳
        public string PartName { get; set; }


        /// 選項名稱：淺橡木色、白色烤漆
        public string OptionName { get; set; }


        /// 色碼（用於顯示色票）
        public string ColorHex { get; set; }


        /// 縮圖
        public string Thumbnail { get; set; }
    }
}
