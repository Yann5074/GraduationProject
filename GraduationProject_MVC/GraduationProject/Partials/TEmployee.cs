using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Models
{
    public partial class TEmployee
    {
        [ForeignKey(nameof(FRoleId))]
        public virtual TEmployeeRole? FRole { get; set; }
        [ForeignKey(nameof(FStatusId))]
        public virtual TEmployeeStatus? FStatus { get; set; }
        [ForeignKey(nameof(FGender))]
        public virtual TGender? FGenderNavigation { get; set; }
    }
}
