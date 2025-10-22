using GraduationProject.Models;

namespace GraduationProject.Models
{
    public partial class TPstatus
    {
 

        // 導航屬性
        public virtual ICollection<TProduct> TProducts { get; set; }
        public virtual ICollection<TProductVariant> TProductVariants { get; set; }

        public TPstatus()
        {
            TProducts = new HashSet<TProduct>();
            TProductVariants = new HashSet<TProductVariant>();
        }
    }
}
