using ApiProject.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TColorOptionTexture
    {
        [ForeignKey(nameof(FColorOptionId))]
        public virtual TPartColorOption ColorOption { get; set; }
    }
}
