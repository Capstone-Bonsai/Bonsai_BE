using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ComplaintImage : BaseEntity
    {
        [ForeignKey("Complaint")]
        public Guid ComplaintId { get; set; }
        public string Image {  get; set; }
        [JsonIgnore]
        public virtual Complaint Complaint { get; set; }
    }
}
