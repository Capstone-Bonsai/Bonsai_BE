using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ServiceOrderViewModels
{
    public class ServiceOrderForGardenerViewModel
    {
        public Guid Id { get; set; }
        public Guid CustomerBonsaiId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Address { get; set; }
        public ServiceOrderStatus ServiceOrderStatus { get; set; }
        [NotMapped]
        public List<string> Image { get; set; } = default!;
        public ServiceType ServiceType { get; set; }
        [NotMapped]
        public Bonsai Bonsai { get; set; } = default!;
    }
}
