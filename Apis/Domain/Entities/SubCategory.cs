using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SubCategory : BaseEntity
    {
        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public virtual Category Category { get; set; }
        public IList<Product> Products { get;}

    }
}
