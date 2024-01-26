using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Staff : BaseEntity
    {
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
