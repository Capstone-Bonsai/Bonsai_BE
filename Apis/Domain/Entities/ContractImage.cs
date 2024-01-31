using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ContractImage:BaseEntity
    {
        [ForeignKey("ServiceOrder")]
        public Guid ServiceOrderId { get; set; }
        public string Url { get; set; }
        public virtual ServiceOrder ServiceOrder { get; set; }
    }
}
