using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ServiceOrderViewModels
{
    public class ResponseServiceOrderModel
    {
        public Guid? OrderId { get; set; }
        public float ResponseGardenSquare { get; set; }
        public float ResponseStandardSquare { get; set; }
    }
}
