using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.OrderDetailModels
{
    public class OrderDetailModel
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

    }
}
