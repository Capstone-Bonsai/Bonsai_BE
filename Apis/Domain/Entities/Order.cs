﻿using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
        public Guid? GardenerId { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; } = new DateTime(2020, 1, 1);
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public double Price { get; set; }
        public double DeliveryPrice { get; set; }
        public double TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string? Note { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual OrderTransaction OrderTransaction { get; set; }
        public IList<OrderDetail> OrderDetails { get; }
        public IList<DeliveryImage> DeliveryImages { get; }
    }
}
