using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class DeliveryFee:BaseEntity
    {
        public int? MaxDistance { get; set; }
        public double? MaxPrice { get; set; }
        public double Fee { get; set; }
        public DeliveryType DeliveryType { get; set; }

    }
}
