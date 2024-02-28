using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class SubCategory : BaseEntity
    {
        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public virtual Category Category { get; set; }
        public IList<Product> Products { get; }

    }
}
