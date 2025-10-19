using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TProductPart

    {
        //[ForeignKey(nameof(FPartId))]
        //public TProductPart PartId { get; set; }

        [ForeignKey(nameof(FProductId))]
        public TProduct ProductId { get; set; }
        public virtual ICollection<TPartColorOption> ColorOptions { get; set; }

    }
}
