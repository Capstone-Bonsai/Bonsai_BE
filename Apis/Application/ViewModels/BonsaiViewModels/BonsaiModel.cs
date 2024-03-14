using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.ViewModels.BonsaiViewModel
{
    public class BonsaiModel
    {
        public Guid CategoryId { get; set; }
        public Guid StyleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? YearOfPlanting { get; set; }
        public int TrunkDimenter { get; set; }
        public float? Height { get; set; }
        public int NumberOfTrunk { get; set; }
        public double Price { get; set; }
        [NotMapped]
        public List<String>? OldImage { get; set; } = default!;
        [NotMapped]
        public List<IFormFile>? Image { get; set; } = default!;
    }
}
