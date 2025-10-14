using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TProductVariant
    {
        [Column("fProductVariantId")] 
        public int ProductVariantId { get; set; }
        [Column("fProductId")] 
        public int ProductId { get; set; }
        [Column("fSKU")] 
        public string? SKU { get; set; }                 // 例：TABLERBLUE10010060
        [Column("fPrice")] 
        public decimal? Price { get; set; }
        [Column("fCost")] 
        public decimal? Cost { get; set; }
        [Column("fStock")] 
        public int? Stock { get; set; }
        [Column("fPStatus")] 
        public int? PStatus { get; set; }
        [Column("fColorId")] 
        public int? ColorId { get; set; }
        [Column("fSizeLabel")] 
        public string? SizeLabel { get; set; }

        public TColor? Color { get; set; }

        [ForeignKey(nameof(FProductId))]
        public TProduct Product { get; set; }

        [ForeignKey(nameof(FProductVariantId))]
        public TProductAsset ProductAsset { get; set; }
    }
}
