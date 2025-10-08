using GraduationProject.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Models
{
    public partial class TMember
    {
        // 1) 導覽屬性（外鍵關聯）
        [ForeignKey(nameof(FGender))]
        public virtual TGender? FGenderNavigation { get; set; }

        [ForeignKey(nameof(FStatus))]
        public virtual TStatus? FStatusNavigation { get; set; }

        [ForeignKey(nameof(FLeveId))]
        public virtual TLevel? FLeveIdNavigation { get; set; }
    }
}
