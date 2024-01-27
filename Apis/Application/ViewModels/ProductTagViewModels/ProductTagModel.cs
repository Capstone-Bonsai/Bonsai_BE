using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ProductTagViewModels
{
    public class ProductTagModel
    {
        public Guid ProductId { get; set; }
        public Guid TagId { get; set; }
    }
}
