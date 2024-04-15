using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ServiceTypeViewModels
{
    public class ServiceTypeModel
    {
        public String? Description { get; set; } = default!;
        public IFormFile? Image { get; set; } = default!;
    }
}
