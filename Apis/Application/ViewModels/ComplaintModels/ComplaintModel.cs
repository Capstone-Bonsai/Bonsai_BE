using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ComplaintModels
{
    public class ComplaintModel
    {
        public Guid ContractId { get; set; }
        public string Detail { get; set; }
        public List<IFormFile>? ListImage { get; set; } 
    }
}
