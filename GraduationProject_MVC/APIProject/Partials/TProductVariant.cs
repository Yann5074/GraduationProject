using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TProductVariant
    {
        [ForeignKey(nameof(FProductId))]
        public TProduct Product { get; set; }

        [ForeignKey(nameof(FProductVariantId))]
        public TProductAsset ProductAsset { get; set; }
    }
}
