using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrderDetail : BaseEntity
    {
        [ForeignKey("Order")]
        public Guid OrderId { get; set; }
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
        public int UnitPrice { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }

    }
}
