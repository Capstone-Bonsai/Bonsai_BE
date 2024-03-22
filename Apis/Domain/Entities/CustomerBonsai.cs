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
    public class CustomerBonsai:BaseEntity
    {
        [ForeignKey("Bonsai")]
        public Guid BonsaiId { get; set; }
        [ForeignKey("CustomerGarden")]
        public Guid CustomerGardenId { get; set; }
        public virtual Bonsai Bonsai { get; set;}
        [JsonIgnore]
        public virtual CustomerGarden CustomerGarden { get; set;}
    }
}
