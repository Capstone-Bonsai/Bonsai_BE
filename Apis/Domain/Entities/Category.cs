using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        [JsonIgnore]
        public IList<Bonsai> Bonsais { get; set; }
        
        [JsonIgnore]
        public IList<CareStep> CareSteps { get; set; }
    }
}
