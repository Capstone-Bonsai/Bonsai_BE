using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class OrderDetail : BaseEntity
    {
        [ForeignKey("Order")]
        public Guid OrderId { get; set; }
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }

    }
}
