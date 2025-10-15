using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TProductVariant
    {


        [ForeignKey(nameof(FPstatus))]
        public virtual TPstatus PStatus { get; set; }


        [ForeignKey(nameof(FColorId))]
        public TColor Color { get; set; }

        [ForeignKey(nameof(FProductId))]
        public TProduct Product { get; set; }

        [ForeignKey(nameof(FProductVariantId))]
        public TProductAsset ProductAsset { get; set; }

       
    }
}
