
using ApiProject.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace ApiProject.Models
{
    public partial class TProduct
    {

        [ForeignKey(nameof(FProductId))]
        public TProduct ProductId { get; set; }
        public virtual ICollection<TProductVariant> ProductVariants { get; set; } = new List<TProductVariant>();
        public virtual ICollection<TProductAsset> ProductAssets { get; set; } = new List<TProductAsset>();

        [ForeignKey(nameof(FCategoryId))]
        public TCategory Category { get; set; }

        [ForeignKey(nameof(FPstatus))]
        public virtual TPstatus PStatus { get; set; }

    }


}
