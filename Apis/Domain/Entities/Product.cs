using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Product : BaseEntity
    {
        [ForeignKey("SubCategory")]
        public Guid SubCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? TreeShape { get; set; }
        public int? AgeRange { get; set; }
        public float? Height { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }

        public virtual SubCategory SubCategory { get; set; }
        public IList<ProductImage> ProductImages { get; set; }
    }
}
