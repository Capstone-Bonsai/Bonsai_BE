using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.CustomerGardenViewModels
{
    public class CustomerGardenViewModel
    {
        public Guid CustomerId { get; set; }
        public string Address { get; set; }
        public float Square { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
