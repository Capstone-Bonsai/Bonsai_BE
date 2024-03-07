namespace Domain.Entities
{
    public class Style : BaseEntity
    {
        public string Name { get; set; }

        public IList<Bonsai> Bonsais { get; set; }
    }
}
