using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Address : BaseEntity
    {
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
        public string AddressName { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
