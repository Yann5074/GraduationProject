using ApiProject.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TProductAsset
    {

        [Column("fAssetId")] 
        public int AssetId { get; set; }
        [Column("fProductId")] 
        public int? ProductId { get; set; }
        [Column("fProductVariantId")] 
        public int? ProductVariantId { get; set; }
        [Column("fPicture")] 
        public string? Picture { get; set; }
        [Column("fUrl")] 
        public string? Url { get; set; }                  // 例：/images/xxx.webp:contentReference[oaicite:9]{index=9}
        [Column("fMimeType")] 
        public string? MimeType { get; set; }        // 例：image/webp
        [Column("fIsPrimary")]
        public bool IsPrimary { get; set; }
        [Column("fSortOrder")] 
        public int SortOrder { get; set; }

        public TProduct? Product { get; set; }
        public TProductVariant? ProductVariant { get; set; }
    }

}
