using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Customer : BaseEntity
    {
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public IList<Address> Addresses { get; set; }
    }
}
