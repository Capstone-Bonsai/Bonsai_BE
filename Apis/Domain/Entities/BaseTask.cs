using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BaseTask:BaseEntity
    {
        [ForeignKey("Tasks")]
        public Guid TaskId { get; set; }
        [ForeignKey("Service")]
        public Guid ServiceId { get; set; }

        public virtual Tasks Task { get; set; }
        public virtual Service Service { get; set; }
    }
}
