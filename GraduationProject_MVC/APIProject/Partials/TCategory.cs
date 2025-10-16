using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TCategory
    {

       
        // 導航屬性
        public virtual TCategory FParentCategory { get; set; }
        public virtual ICollection<TCategory> InverseFParentCategory { get; set; }
        public virtual ICollection<TProduct> TProducts { get; set; }

    }
}
