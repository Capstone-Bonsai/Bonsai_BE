using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class DayInWeek:BaseEntity
    {
        public DayType DayType { get; set; }
        public IList<ServiceDay> ServiceDays { get; set;}
    }
}
