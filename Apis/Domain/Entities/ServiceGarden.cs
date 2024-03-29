using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ServiceGarden:BaseEntity
    {
        [ForeignKey("CustomerGarden")]
        public Guid CustomerGardenId { get; set; }
        [ForeignKey("Service")]
        public Guid ServiceId { get; set; }
        public Guid? CustomerBonsaiId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double? TemporaryPrice { get; set; }
        public double? TemporarySurchargePrice { get; set; }
        public double? TemporaryTotalPrice { get; set; }
        public int? TemporaryGardener { get; set; }
        public ServiceGardenStatus ServiceGardenStatus { get; set; }
        public string? Note { get; set; }
        public virtual CustomerGarden CustomerGarden { get; set; }
        public virtual Service Service { get; set; }
    }
}
