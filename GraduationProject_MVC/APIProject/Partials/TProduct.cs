
using ApiProject.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace ApiProject.Models
{
    public partial class TProduct
    {

        [ForeignKey(nameof(FProductId))]
        public TProduct ProductId { get; set; }
        public virtual ICollection<TProductVariant> ProductVariants { get; set; } = new List<TProductVariant>();
        public virtual ICollection<TProductAsset> ProductAssets { get; set; } = new List<TProductAsset>();

        [ForeignKey(nameof(FCategoryId))]
        public TCategory Category { get; set; }

        [ForeignKey(nameof(FPstatus))]
        public virtual TPstatus PStatus { get; set; }


        [NotMapped]
        public string MainImageUrl => ProductAssets?
            .Where(a => a.FIsPrimary == true)
            .OrderBy(a => a.FSortOrder)
            .Select(a => a.FUrl)
            .FirstOrDefault() ?? "/images/default.png";

        [NotMapped]
        public int TotalStock => ProductVariants?.Sum(v => v.FStock ?? 0) ?? 0;

        [NotMapped]
        public bool IsAvailable => FPstatus == 1 && TotalStock > 0;

        [NotMapped]
        public decimal? MinPrice => ProductVariants?
            .Where(v => v.FPrice.HasValue)
            .Min(v => v.FPrice);

        [NotMapped]
        public decimal? MaxPrice => ProductVariants?
            .Where(v => v.FPrice.HasValue)
            .Max(v => v.FPrice);

    }


}
