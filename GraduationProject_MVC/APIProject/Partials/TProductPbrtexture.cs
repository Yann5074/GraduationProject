using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TProductPbrtexture
    {
        [ForeignKey(nameof(FProductId))]
        public virtual TProduct Product { get; set; }

        [ForeignKey(nameof(FProductVariantId))]
        public TProductVariant ProductVariant { get; set; }
    }
}
