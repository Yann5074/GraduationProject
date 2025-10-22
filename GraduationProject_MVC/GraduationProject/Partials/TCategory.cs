using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Models
{
    public partial class TCategory
    {



        // 導航屬性
        [ForeignKey(nameof(FParentCategoryId))]
        public virtual TCategory ParentCategory { get; set; }

        public virtual ICollection<TCategory> SubCategories { get; set; }

        public virtual ICollection<TProduct> TProducts { get; set; }

        public TCategory()
        {
            SubCategories = new HashSet<TCategory>();
            TProducts = new HashSet<TProduct>();
        }
    }
}
