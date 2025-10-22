using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Models
{
    public partial class TProductPart
    {


        // 導航屬性
        [ForeignKey(nameof(FProductId))]
        public virtual TProduct FProduct { get; set; }

        public virtual ICollection<TPartColorOption> PartColorOptions { get; set; }

        public TProductPart()
        {
            PartColorOptions = new HashSet<TPartColorOption>();
        }
    }
}
