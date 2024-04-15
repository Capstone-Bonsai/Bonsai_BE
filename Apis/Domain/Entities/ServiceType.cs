using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ServiceType: BaseEntity
    {
         public string TypeName { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public TypeEnum TypeEnum { get; set; }
    }
}
