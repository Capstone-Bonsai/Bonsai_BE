using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace Domain.Entities
{
    public class Product : BaseEntity
    {
        [ForeignKey("SubCategory")]
        public Guid SubCategoryId { get; set; }
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
        public string NameUnsign { get; private set; }
        public string Description { get; set; }
        public string? TreeShape { get; set; }
        public int? AgeRange { get; set; }
        public float? Height { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public bool isDisable { get; set; }

        public virtual SubCategory SubCategory { get; set; }
        public IList<ProductImage> ProductImages { get; set; }
        private string RemoveDiacritics(string text)
        {
            string normalized = text.Normalize(NormalizationForm.FormD);
            StringBuilder result = new StringBuilder();

            foreach (char c in normalized)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

                // Replace "đ" and "d" with "d"
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
