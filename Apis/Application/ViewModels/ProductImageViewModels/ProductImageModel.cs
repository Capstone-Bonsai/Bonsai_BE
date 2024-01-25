using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ProductImageViewModels
{
    public class ProductImageModel
    {
        public Guid? ProductId { get; set; }
        public List<IFormFile> Image { get; set; } = default!;
    }
}
