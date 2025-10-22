using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace GraduationProject.Models
{
    public partial class TProductVariant
    {



        [ForeignKey(nameof(FPstatus))]
        public virtual TPstatus FPstatusNavigation { get; set; }

        [ForeignKey(nameof(FProductId))]
        public TProduct product { get; set; }

    
        [ForeignKey(nameof(FColorId))]
        public TColor Color { get; set; }

        public ICollection<TProductAsset> TProductAssets { get; set; } = new List<TProductAsset>();

    }
}


