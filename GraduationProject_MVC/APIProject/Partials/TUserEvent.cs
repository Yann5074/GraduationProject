using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TUserEvent
    {
        [ForeignKey(nameof(FProductVariantId))]
        public TProductVariant ProductVariant { get; set; }

        [ForeignKey(nameof(FProductId))]
        public TProduct Product { get; set; }
    }
}
