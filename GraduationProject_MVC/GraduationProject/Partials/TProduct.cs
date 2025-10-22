
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace GraduationProject.Models
{
    public partial class TProduct
    {


        // 導航屬性
        [ForeignKey(nameof(FCategoryId))]
        public virtual TCategory FCategory { get; set; }

        [ForeignKey(nameof(FPstatus))]
        public virtual TPstatus FPstatusNavigation { get; set; }

        public virtual ICollection<TProductAsset> ProductAssets { get; set; }
        public virtual ICollection<TProductVariant> ProductVariants { get; set; }
        public virtual ICollection<TProductPart> ProductParts { get; set; }


    }


}
