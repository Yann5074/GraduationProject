using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TProductVariant
    {
        [ForeignKey(nameof(FProductId))]
        public TProduct Product { get; set; }

        [ForeignKey(nameof(FProductVariantId))]
        public TProductAsset ProductAsset { get; set; }


        [ForeignKey(nameof(FPstatus))]
        public virtual TPstatus PStatus { get; set; }



        [ForeignKey(nameof(FColorId))]
        public TColor Color { get; set; }

        public ICollection<TProductAsset> TProductAssets { get; set; } = new List<TProductAsset>();
    }
}
