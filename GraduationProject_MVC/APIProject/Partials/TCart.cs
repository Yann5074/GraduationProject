using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TCart
    {
        [ForeignKey(nameof(FCartId))]
        public ICollection<TCartItem> CartItem { get; set; }
    }
}
