using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Models
{
    public partial class TProductVariant
    {
        [ForeignKey(nameof(FProductId))]
        public TProduct product { get; set; }
    }
}
