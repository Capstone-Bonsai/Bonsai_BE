using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Domain.Entities
{
    public class Bonsai : BaseEntity
    {
        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }
        [ForeignKey("Style")]
        public Guid StyleId { get; set; }
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NameUnsign = RemoveDiacritics(value);
            }
        }
        public string Code { get; private set; }
        public string NameUnsign { get; private set; }
        public string Description { get; set; }
        public int? YearOfPlanting { get; set; }
        public int TrunkDimenter { get; set; }
        public float? Height { get; set; }
        public int NumberOfTrunk { get; set; }
        public double Price { get; set; }
        public bool isDisable { get; set; }
        public bool? isSold { get; set; }
        public virtual Category Category { get; set; }
        public virtual Style Style { get; set; }
        public virtual CustomerBonsai?  CustomerBonsai { get; set; }
        [JsonIgnore]
        public IList<OrderDetail> OrderDetails { get; set; }
        public IList<BonsaiImage> BonsaiImages { get; set; }
        private string RemoveDiacritics(string text)
        {
            string normalized = text.Normalize(NormalizationForm.FormD);
            StringBuilder result = new StringBuilder();

            foreach (char c in normalized)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (c == 'đ' || c == 'Đ')
                {
                    result.Append('d');
                }
                else if (category != UnicodeCategory.NonSpacingMark)
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}
