using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Service:BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public double StandardPrice { get; set; }
        public string Image {  get; set; }
        public ServiceType ServiceType { get; set; }

        public IList<Contract> Contracts { get; set; }
        public IList<ServiceBaseTask> ServiceBaseTasks { get; set; }

    }
}
