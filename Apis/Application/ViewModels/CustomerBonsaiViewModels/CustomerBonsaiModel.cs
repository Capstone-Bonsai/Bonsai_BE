using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.CustomerBonsaiViewModels
{
    public class CustomerBonsaiModel
    {
        public Guid BonsaiId { get; set; }
        public Guid CustomerGardenId { get; set; }
    }
}
