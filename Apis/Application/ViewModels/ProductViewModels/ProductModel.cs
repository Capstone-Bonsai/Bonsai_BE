using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.ViewModels.ProductViewModels
{
    public class ProductModel
    {
        public Guid SubCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? TreeShape { get; set; }
        public int? AgeRange { get; set; }
        public float? Height { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        [NotMapped]
        public List<IFormFile> Image { get; set; } = default!;
    }
}
