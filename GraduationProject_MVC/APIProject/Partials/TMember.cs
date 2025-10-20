using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TMember
    {
        [ForeignKey(nameof(FLeveId))]
        public TLevel Level { get; set; }
    }
}
