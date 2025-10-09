using GraduationProject.Models;

namespace GraduationProject.Partials
{
    public partial class TColor
    {
        public int FColorId { get; set; }
        public string FColorName { get; set; }
        public virtual ICollection<TProductVariant> ProductVariants { get; set; } = new List<TProductVariant>();
    }
}
