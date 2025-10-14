using ApiProject.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TColor
    {
        [Column("fColorId")] 
        public int ColorId { get; set; }
        [Column("fColorName")] 
        public string? ColorName { get; set; }
        [Column("fColorCode")] 
        public string? ColorCode { get; set; }      // HEX（例如 FFFFFF）
    }
}
