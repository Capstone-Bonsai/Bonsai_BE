﻿using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.OrderViewModels
{
    public class OrderViewModel
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? StaffId { get; set; }
        public string Address { get; set; }
        public string Province { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public double Price { get; set; }
        public double DeliveryPrice { get; set; }
        public double TotalPrice { get; set; }
        public string OrderStatus { get; set; }
        public string? Note { get; set; }
        public string OrderType { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual OrderTransaction OrderTransaction { get; set; }
        public IList<OrderDetail> OrderDetails { get; }
    }
}