
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Models
{
    public partial class TProduct
    {
        [ForeignKey(nameof(FProductId))]
        public TProductVariant ProductVariant { get; set; }

        [ForeignKey(nameof(FProductId))]
        public TProductAsset ProductAsset { get; set; }

        [ForeignKey(nameof(FCategoryId))]
        public TCategory Category  { get; set; }


    }


}
