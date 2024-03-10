using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CustomerGardenImage:BaseEntity
    {
        [ForeignKey("CustomerGarden")]
        public Guid CustomerGardenId { get; set; }
        public string Image {  get; set; }
        [JsonIgnore]
        public virtual CustomerGarden CustomerGarden { get; set; }

    }
}
