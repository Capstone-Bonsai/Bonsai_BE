using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class BonsaiImage : BaseEntity
    {
        [ForeignKey("Bonsai")]
        public Guid BonsaiId { get; set; }
        public string ImageUrl { get; set; }
        [JsonIgnore]
        public virtual Bonsai Bonsai { get; set; }
    }
}
