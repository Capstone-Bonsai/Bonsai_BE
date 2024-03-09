using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ServiceBaseTask:BaseEntity
    {
        [ForeignKey("BaseTask")]
        public Guid BaseTaskId { get; set; }
        [ForeignKey("Service")]
        public Guid ServiceId { get; set; }
        public virtual BaseTask BaseTask { get; set; }
        public virtual Service Service { get; set; }
    }
}
