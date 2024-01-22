using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ProductImage:BaseEntity
    {
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; }
        public virtual Product Product { get; set; }
    }
}
