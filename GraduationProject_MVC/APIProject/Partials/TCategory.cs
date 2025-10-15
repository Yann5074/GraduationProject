using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TCategory
    {

        
        public int CategoryId { get; set; }

        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool? IsActive { get; set; }
        public int? SortOrder { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }

        // 導航屬性
        public virtual TCategory FParentCategory { get; set; }
        public virtual ICollection<TCategory> InverseFParentCategory { get; set; }
        public virtual ICollection<TProduct> TProducts { get; set; }

    }
}
