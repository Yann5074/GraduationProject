using ApiProject.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models
{
    public partial class TColor
    {

        public int ColorId { get; set; }

        public string? ColorName { get; set; }

        public string? ColorCode { get; set; }      // HEX（例如 FFFFFF）
    }
}
