using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.DeliveryFeeViewModels
{
    public class FeeViewModel
    {
        public string Destination_addresses { get; set; }
        public string Origin_addresses { get; set; }
        public int Distance { get; set; }
        public double PriceAllBonsai { get; set; }
        public int DurationHour { get; set; }
        public int DurationMinute { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public double DeliveryFee { get; set; }
        public double FinalPrice { get; set; }
        public DeliverySize DeliverySize { get; set; }  
    }
}
