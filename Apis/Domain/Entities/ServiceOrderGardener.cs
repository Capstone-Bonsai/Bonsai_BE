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
    public class ServiceOrderGardener:BaseEntity
    {
        [ForeignKey("ServiceOrder")]
        public Guid ServiceOrderId { get; set; }
        [ForeignKey("Gardener")]
        public Guid GardenerId { get; set; }
        public bool HasRequest { get; set; }
        public string? Note { get; set; }
        [JsonIgnore]
        public virtual ServiceOrder ServiceOrder { get; set; }
        public virtual Gardener Gardener { get; set; }  
    }
}
