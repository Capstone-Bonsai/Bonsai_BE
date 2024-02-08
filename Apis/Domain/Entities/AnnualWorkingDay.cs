using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AnnualWorkingDay:BaseEntity
    {
        [ForeignKey("ServiceOrder")]
        public Guid ServiceOrderId { get; set; }
        [ForeignKey("Gardener")]

        public Guid GardenerId { get; set; }

        public DateTime Date {  get; set; }


        public virtual ServiceOrder ServiceOrder { get; set; }
        public virtual Gardener Gardener { get; set; }


    }
}
