using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TCartItem
    {
        [ForeignKey(nameof(FProductVariantId))]
        public TProductVariant ProductVariant { get; set; }
    }
}
