﻿using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CustomerGarden:BaseEntity
    {
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
        public string Address { get; set; }
        public float Square { get; set; }
        public double? TemporaryPrice { get; set; }
        public double? TemporarySurchargePrice { get; set; }
        public double? TemporaryTotalPrice { get; set; }
        public CustomerGardenStatus CustomerGardenStatus { get; set; }
        public string? Note { get; set; }
        public virtual Customer Customer { get; set; }
        public IList<CustomerBonsai > CustomerBonsais { get; set; }
        public IList<CustomerGardenImage> CustomerGardenImages { get; set; }
        public IList<Contract> Contracts { get; set; }
    }
}