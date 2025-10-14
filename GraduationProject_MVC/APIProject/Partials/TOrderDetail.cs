using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TOrderDetail
    {
        [ForeignKey(nameof(FProductVariantId))]
        public TProductVariant ProductVariant { get; set; }
    }
}
