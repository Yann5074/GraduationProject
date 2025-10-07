using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Models
{
    public partial class TOrderDetail
    {
        //用於 Navigation Property 與宣告外鍵
        [ForeignKey(nameof(FProductVariantId))]
        public TProductVariant productVariant {  get; set; }
    }
}
