using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.DeliveryFeeViewModels
{
    public class DeliveryModel
    {
        public string destination { get; set; }
        public IList<Guid> listBonsaiId { get; set; }   
    }
}
