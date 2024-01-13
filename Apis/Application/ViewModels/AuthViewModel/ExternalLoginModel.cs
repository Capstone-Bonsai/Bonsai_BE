using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.AuthViewModel
{
    public class ExternalLoginModel
    {
        public string? Provider { get; set; }
        public string? tokenId { get; set; }
    }
}
