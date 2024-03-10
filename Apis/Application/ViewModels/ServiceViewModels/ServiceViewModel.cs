using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.ViewModels.ServiceViewModels
{
    public class ServiceViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public double StandardPrice { get; set; }
        public string Image { get; set; }
        public string ServiceType { get; set; }
        public IList<ServiceBaseTask> ServiceBaseTasks { get; set; }
    }
}
