using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.SubCategoryViewModels
{
    public class SubCategoryModel
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }
}
