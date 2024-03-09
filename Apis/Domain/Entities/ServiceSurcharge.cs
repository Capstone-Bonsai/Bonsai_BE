using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ServiceSurcharge:BaseEntity
    {
        public float Distance { get; set; }
        public double Price { get; set; }
    }
}
