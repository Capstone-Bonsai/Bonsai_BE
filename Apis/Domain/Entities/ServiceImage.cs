using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ServiceImage:BaseEntity
    {
        [ForeignKey("ServiceOrder")]
        public Guid ServiceOrderId { get; set; }
        public virtual ServiceOrder ServiceOrder { get; set; }
        public string ImageUrl { get; set; }
    }
}
