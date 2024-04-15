using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class GardenCareTask:BaseEntity
    {
        [ForeignKey("ServiceOrder")]
        public Guid ServiceOrderId { get; set; }
        [ForeignKey("BaseTask")]
        public Guid BaseTaskId { get; set; }
        public string? Note { get; set; }
        public DateTime? CompletedTime { get; set; }
        public virtual ServiceOrder ServiceOrder { get; set; }
        public virtual BaseTask BaseTask { get; set; }

    }
}
