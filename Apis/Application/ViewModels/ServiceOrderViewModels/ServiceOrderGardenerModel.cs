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
    public class ServiceOrderGardenerModel
    {
        public Guid ServiceOrderId { get; set; }
        public List<Guid> GardenerIds { get; set; }
    }
}
