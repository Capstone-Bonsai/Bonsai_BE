using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ProductViewModels
{
    public class FilterProductModel
    {
        public string? keyword { get; set; }
        public Guid? subCategory { get; set; }
        public List<Guid>? tag { get; set; }
        public double? minPrice { get; set; }
        public double? maxPrice { get; set; }
        public string? treeShape { get; set; }
    }
}
