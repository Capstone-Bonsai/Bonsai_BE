using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
        public Guid? StaffId { get; set; }
        public string Address { get; set; }
        public string Province { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public double Price { get; set; }
        public double DeliveryPrice { get; set; }
        public double TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string? Note { get; set; }
        public OrderType OrderType { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual OrderTransaction OrderTransaction { get; set; }
        public IList<OrderDetail> OrderDetails { get; }
    }
}
