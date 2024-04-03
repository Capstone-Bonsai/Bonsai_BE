using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ServiceGardenViewModels
{
    public class ServiceGardenModel
    {
        public Guid CustomerGardenId { get; set; }
        public Guid? CustomerBonsaiId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Note { get; set; }
    }
}
