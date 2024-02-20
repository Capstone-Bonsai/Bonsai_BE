using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Service:BaseEntity
    {
        public string Name { get; set; }
        public string Image {  get; set; }
        public string Description { get; set; }
        public float StandardSqure { get; set; }
        public double StandardPrice { get; set; }
        public float? DiscountPercent { get; set; }
        public int? ImplementationTime { get; set; }
        public ServiceType ServiceType { get; set; }

        public IList<BaseTask> BaseTasks { get; set; }
        [JsonIgnore]
        public IList<ServiceOrder> ServiceOrders { get; set; }

    }
}
