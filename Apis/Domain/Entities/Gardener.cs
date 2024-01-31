using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Gardener : BaseEntity
    {
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public IList<AnnualWorkingDay> AnnualWorkingDay { get; set; }
    }
}
