using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TCategory
    {
        [Key]
        [Column("fCategoryId")]
        public int CategoryId { get; set; }

        [Column("fName")] public string Name { get; set; } = "";
        [Column("fParentCategoryId")] public int ParentCategoryId { get; set; }
        [Column("fIsActive")] public bool IsActive { get; set; }
        [Column("fSortOrder")] public int SortOrder { get; set; }


        // （可選）導覽


        public ICollection<TProduct> Products { get; set; } = new List<TProduct>();

    }
}
