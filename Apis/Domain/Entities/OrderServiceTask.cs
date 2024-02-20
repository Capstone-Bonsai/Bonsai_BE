using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrderServiceTask:BaseEntity
    {
        [ForeignKey("ServiceOrder")]
        public Guid ServiceOrderId { get; set; }
        [ForeignKey("Tasks")]
        public Guid TaskId { get; set; }
        public ServiceTaskStatus  ServiceTaskStatus { get; set; }
        public string Note { get; set; }
        public DateTime? CompletedDate { get; set; }
        public virtual ServiceOrder ServiceOrder { get; set; }
        public virtual Tasks Task { get; set; }

    }
}
