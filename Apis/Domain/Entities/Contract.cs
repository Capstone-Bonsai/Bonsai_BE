using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public  class Contract : BaseEntity
    {
        [ForeignKey ("CustomerGarden")]
        public Guid CustomerGardenId { get; set; }
        [ForeignKey("Service")]
        public Guid ServiceId { get; set; }
        public Guid? CustomerBonsaiId { get; set; }
        public string CustomerName { get; set; }      
        public string CustomerPhoneNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float GardenSquare { get; set; }
        public string Address { get; set; }
        public double StandardPrice { get; set; }

        public double SurchargePrice { get; set; }
        public double ServicePrice { get; set; }
        public double TotalPrice { get; set; }
        public ContractStatus ContractStatus { get; set; }
        public ContractType ContractType { get; set; }
        public int NumberOfGardener { get; set; }



        public virtual CustomerGarden CustomerGarden { get; set; }
        public virtual Service Service { get; set; }

        public IList<BonsaiCareStep> BonsaiCareSteps { get; set; }
        public IList<ContractImage> ContractImages { get; set; }
        public IList<GardenCareTask> GardenCareTasks { get; set; }
        public IList<ContractGardener> ContractGardeners { get; set; }

        public IList<ContractTransaction> ContractTransactions { get; set; }


    }
}
