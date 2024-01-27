using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.DeliveryFeeViewModels
{
    public class DeliveryFeeViewModel
    {
        public IList<string> MaxPrice { get; set; }

        public IList<DeliveryItemViewModel> DeliveryItemViewModel { get; set;}

    }
}
