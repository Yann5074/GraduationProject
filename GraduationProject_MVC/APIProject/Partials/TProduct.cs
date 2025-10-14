
using ApiProject.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace ApiProject.Models
{
    public partial class TProduct
    {
        [Column("fProductId")] 
        public int ProductId { get; set; }
        [Column("fName")] 
        public string? Name { get; set; }
        [Column("fCategoryId")] 
        public int CategoryId { get; set; }
        [Column("fWarrantyMonth")] 
        public int? WarrantyMonth { get; set; }
        [Column("fDescription")] 
        public string? Description { get; set; }
        [Column("fPStatus")] 
        public int? PStatus { get; set; }

        public ICollection<TProductVariant> Variants { get; set; } = new List<TProductVariant>();

        public ICollection<TProductAsset> Assets { get; set; } = new List<TProductAsset>();
        public TCategory? Category { get; set; }

       
    }


}
