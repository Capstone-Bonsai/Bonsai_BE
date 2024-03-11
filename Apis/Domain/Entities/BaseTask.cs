using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BaseTask:BaseEntity
    {
        public string Name { get; set; }
        public string Detail { get; set; }
        [JsonIgnore]
        public IList <GardenCareTask> GardenCareTasks { get; set; }
        [JsonIgnore]
        public IList<ServiceBaseTask> ServiceBaseTasks { get; set; }
    }
}
