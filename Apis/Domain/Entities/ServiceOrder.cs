using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ServiceOrder:BaseEntity
    {
        [ForeignKey ("Service")]
        public Guid ServiceId { get; set; }
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }


        public Guid? StaffId { get; set; }
        public ServiceType ServiceType { get; set; }
        public string Address { get; set; }
        public float StandardSquare { get; set; }
        public double StandardPrice { get; set; }
        public float? DiscountPercent { get; set; }
        public int? ImplementationTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float GardenSquare { get; set; }
        public float ExpectedWorkingUnit { get; set; }
        public double TemporaryPrice { get; set; }
        public double TemporaryTotalPrice { get; set; } //giá sau khi trù discount

        public Guid? OrderId { get; set; }
        public double? OrderPrice { get; set; }
        public float? ResponseGardenSquare { get; set; }
        public float? ResponseStandardSquare { get; set; }
        public double? ResponsePrice { get; set; }
        public float? ResponseWorkingUnit { get; set; }
        public double? ResponseTotalPrice { get; set; }
        public double? ResponseFinalPrice { get; set; }                                                                                                                                                          
        public int? NumberGardener { get; set; }
        public ServiceStatus ServiceStatus { get; set; }

        public virtual Service Service { get; set; }
        public virtual Customer Customer { get; set; }

        public IList<Complain> Complains { get; set; }
        public IList<ServiceTransaction> ServiceTransactions { get; set; }
        public IList<AnnualWorkingDay> AnnualWorkingDays { get; set; }
        public IList<ServiceImage> ServiceImages { get; set; }
        public IList<OrderServiceTask> OrderServiceTasks { get; set; }

        public IList<ContractImage> ContractImages { get; set; }
    }
}
