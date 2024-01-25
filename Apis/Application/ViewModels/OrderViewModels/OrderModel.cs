using Application.ViewModels.OrderDetailModels;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.OrderViewModels
{
    public class OrderModel
    {
        public OrderInfoModel? OrderInfo { get; set; }
        public string Address { get; set; }
        public string Province { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public string? Note { get; set; }
        public IList<OrderDetailModel> ListProduct { get; set; }

    }
}
