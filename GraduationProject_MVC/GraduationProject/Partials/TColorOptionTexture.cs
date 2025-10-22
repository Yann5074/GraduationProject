using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Models
{
    public partial class TColorOptionTexture
    {

 


        // 導航屬性
        [ForeignKey(nameof(FColorOptionId))]
        public virtual TPartColorOption FColorOption { get; set; }
    }
}
