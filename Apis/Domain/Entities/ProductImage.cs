using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; }
        public virtual Product Product { get; set; }
    }
}
