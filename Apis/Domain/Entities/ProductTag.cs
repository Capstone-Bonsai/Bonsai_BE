using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ProductTag : BaseEntity
    {
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        [ForeignKey("Tag")]
        public Guid TagId { get; set; }
        public virtual Product Product { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
