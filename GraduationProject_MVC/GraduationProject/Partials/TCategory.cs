using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Models
{
    public partial class TCategory
    {
        public virtual ICollection<TProduct> Products { get; set; } = new List<TProduct>();

        [ForeignKey(nameof(FParentCategoryId))]
        public virtual TCategory ParentCategory { get; set; }

        public virtual ICollection<TCategory> SubCategories { get; set; } = new List<TCategory>();
    }
}
