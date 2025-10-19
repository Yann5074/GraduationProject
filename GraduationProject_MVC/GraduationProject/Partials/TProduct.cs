
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace GraduationProject.Models
{
    public partial class TProduct
    {
        public virtual ICollection<TProductVariant> ProductVariants { get; set; } = new List<TProductVariant>();
        public virtual ICollection<TProductAsset> ProductAssets { get; set; } = new List<TProductAsset>();

        [ForeignKey(nameof(FCategoryId))]
        public TCategory Category  { get; set; }


        [ForeignKey(nameof(FPstatus))]
        public virtual TPstatus PStatus { get; set; }


    }


}
