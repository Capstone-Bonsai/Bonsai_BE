using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ServiceOrderViewModels
{
    public class ResponseServiceOrderModel
    {
        public Guid? OrderId { get; set; }
        public float ResponseGardenSquare { get; set; }
        public float ResponseStandardSquare { get; set; }
        [NotMapped]
        public List<DayType> ServiceDays { get; set; } = default!;
    }
}
