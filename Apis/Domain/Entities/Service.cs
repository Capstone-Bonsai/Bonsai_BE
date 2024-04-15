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
    public class Service:BaseEntity
    {
        [ForeignKey("ServiceType")]
        public Guid ServiceTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; } = false;
        public string Image {  get; set; }
        [JsonIgnore]
        public IList<ServiceOrder> ServiceOrder { get; set; }
        public IList<ServiceBaseTask> ServiceBaseTasks { get; set; }
        public virtual ServiceType ServiceType { get; set; }

    }
}
