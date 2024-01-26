using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class SubCategory : BaseEntity
    {
        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public virtual Category Category { get; set; }
        public IList<Product> Products { get; }

    }
}
