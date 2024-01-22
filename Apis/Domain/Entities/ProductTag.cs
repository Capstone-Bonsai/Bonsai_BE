using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ProductTag:BaseEntity
    {
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        [ForeignKey("Tag")]
        public Guid TagId { get; set; }
        public virtual Product Product { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
