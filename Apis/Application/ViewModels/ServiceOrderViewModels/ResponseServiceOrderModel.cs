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
        public double? OrderPrice { get; set; }
        public float ResponseGardenSquare { get; set; }
        public float ResponseStandardSquare { get; set; }
        public double ResponsePrice { get; set; }
        public float ResponseWorkingUnit { get; set; }
        public double ResponseTotalPrice { get; set; }
        public double ResponseFinalPrice { get; set; }
        public int NumberGardener { get; set; }
    }
}
