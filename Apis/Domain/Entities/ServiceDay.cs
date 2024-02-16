using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ServiceDay:BaseEntity
    {
        [ForeignKey("DayInWeek")]
        public Guid DayInWeekId { get; set; }
        [ForeignKey("ServiceOrder")]
        public Guid ServiceOrderId { get; set; }

        public virtual DayInWeek DayInWeek { get; set; }
        public virtual ServiceOrder ServiceOrder { get; set; }

    }
}
