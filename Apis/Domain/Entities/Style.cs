using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class Style : BaseEntity
    {
        public string Name { get; set; }
        [JsonIgnore]
        public IList<Bonsai> Bonsais { get; set; }
    }
}
