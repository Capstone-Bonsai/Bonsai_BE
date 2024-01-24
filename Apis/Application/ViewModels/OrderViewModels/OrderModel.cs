using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.OrderViewModels
{
    public class OrderModel
    {
        public Guid CustomerId { get; set; }
        public Guid StaffId { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; }
        public double ExpectedDeliveryDate { get; set; }
        public double Price { get; set; }
        public double DeliveryPrice { get; set; }
        public double TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string? Note { get; set; }
        public OrderType OrderType { get; set; }
    }
}
