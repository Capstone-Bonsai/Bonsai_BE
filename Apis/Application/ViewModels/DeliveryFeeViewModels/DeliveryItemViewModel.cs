using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.DeliveryFeeViewModels
{
    public class DeliveryItemViewModel
    {
        public string Type { get; set; }
        public string MaxDistance { get; set; }
        public double VehicalPrice1 { get; set; }
        public double VehicalPrice2 { get; set; }
        public double VehicalPrice3 { get; set; }

    }
}
