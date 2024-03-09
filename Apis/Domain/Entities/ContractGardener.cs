using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ContractGardener:BaseEntity
    {
        [ForeignKey("Contract")]
        public Guid ContractId { get; set; }
        [ForeignKey("Gardener")]
        public Guid GardenerId { get; set; }
        public bool HasRequest { get; set; }
        public string? Note { get; set; }
        public virtual Contract Contract { get; set; }
        public virtual Gardener Gardener { get; set; }  
    }
}
