using ApiProject.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TProductAsset
    {

        [ForeignKey(nameof(FProductId))]
        public virtual TProduct Product { get; set; }

        [ForeignKey(nameof(FProductVariantId))]
        public TProductVariant ProductVariant { get; set; }
    }

}
