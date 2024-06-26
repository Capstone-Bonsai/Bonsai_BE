﻿using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ServiceOrder : BaseEntity
    {
        [ForeignKey("CustomerGarden")]
        public Guid CustomerGardenId { get; set; }
        [ForeignKey("Service")]
        public Guid ServiceId { get; set; }
        public Guid? CustomerBonsaiId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public int Distance { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float GardenSquare { get; set; }
        public string Address { get; set; }
        public double TotalPrice { get; set; }
        public ServiceOrderStatus ServiceOrderStatus { get; set; } = ServiceOrderStatus.Pending;
        public virtual CustomerGarden CustomerGarden { get; set; }
        public virtual Service Service { get; set; }
        public IList<BonsaiCareStep> BonsaiCareSteps { get; set; }
        public IList<Contract> Contract { get; set; }
        public IList<GardenCareTask> GardenCareTasks { get; set; }
        public IList<ServiceOrderGardener> ServiceOrderGardener { get; set; }
        public IList<ServiceOrderTransaction> ContractTransactions { get; set; }
        public IList<Complaint> Complaints { get; set; }
    }
}
