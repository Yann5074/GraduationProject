using ApiProject.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TPartColorOption
    {

        [ForeignKey(nameof(FPartId))]
        public virtual TProductPart Part { get; set; }

        public virtual ICollection<TColorOptionTexture> Textures { get; set; }
        public TPartColorOption()
        {
            Textures = new HashSet<TColorOptionTexture>();
        }
    }
}
