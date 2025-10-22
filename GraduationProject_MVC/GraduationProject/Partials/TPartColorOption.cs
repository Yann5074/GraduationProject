using GraduationProject.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Models
{
    public partial class TPartColorOption
    {
  
        // 導航屬性
        [ForeignKey(nameof(FPartId))]
        public virtual TProductPart FPart { get; set; }

        public virtual ICollection<TColorOptionTexture> ColorOptionTextures { get; set; }

        public TPartColorOption()
        {
            ColorOptionTextures = new HashSet<TColorOptionTexture>();
        }
    }
}
