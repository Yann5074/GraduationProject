using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Models
{
    public partial class TLeave
    {
        [ForeignKey(nameof(FStatusId))]
        public virtual TRequestStatus FStatus { get; set; }
    }
}
