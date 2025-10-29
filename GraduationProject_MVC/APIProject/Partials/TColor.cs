using ApiProject.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TColor
    {

        public virtual ICollection<TProductVariant>? ProductVariants { get; set; }
    }
}
