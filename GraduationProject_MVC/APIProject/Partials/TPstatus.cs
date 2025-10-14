using ApiProject.Models;

namespace ApiProject.Models
{
    public partial class TPstatus
    {
        public int FPStatus { get; set; }          // 主鍵
        public string FPStatusName { get; set; }   // 你要搜尋/顯示的名稱
        public virtual ICollection<TProduct> Products { get; set; } = new List<TProduct>();
    }
}
