using Domain.Entities;
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
        public string DeliveryType { get; set; }
        public int Distance { get; set; }
        public double Price { get; set; }
        public DeliveryFee deliveryFee { get; set; } = new DeliveryFee();
    }
}
