using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
