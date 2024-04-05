using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CustomerGarden:BaseEntity
    {
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
        public string Address { get; set; }
        public float Square { get; set; }
  
        public virtual Customer Customer { get; set; }
        public IList<CustomerBonsai > CustomerBonsais { get; set; }
        public IList<CustomerGardenImage> CustomerGardenImages { get; set; }
        [JsonIgnore]
        public IList<ServiceGarden> ServiceGarden { get; set; }
    }
}
