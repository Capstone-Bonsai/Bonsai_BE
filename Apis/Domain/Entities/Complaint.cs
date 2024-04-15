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
    public class Complaint:BaseEntity
    {
        [ForeignKey("ServiceOrder")]
        public Guid ServiceOrderId { get; set; }
        public string Detail { get; set; }
        public ComplaintStatus ComplaintStatus { get; set; } = ComplaintStatus.Request;
        public string? CancelReason { get; set; }
        [JsonIgnore]
        public virtual ServiceOrder ServiceOrder { get; set; }
        public IList<ComplaintImage> ComplaintImages { get; set; }

    }
}
