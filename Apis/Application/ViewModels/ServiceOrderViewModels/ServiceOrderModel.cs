using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ServiceOrderViewModels
{
    public class ServiceOrderModel
    {
        public Guid ServiceId { get; set; }
        public string Address { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float GardenSquare { get; set; }
        public float ExpectedWorkingUnit { get; set; }
        public double TemporaryPrice { get; set; }
        public double TemporaryTotalPrice { get; set; }
        [NotMapped]
        public List<IFormFile>? Image { get; set; } = default!;
    }
}
