using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ContractViewModels
{
    public class ContractModel
    {
        public Guid ServiceGardenId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public int Distance { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float GardenSquare { get; set; }
        public string Address { get; set; }
        public double StandardPrice { get; set; }
        public double SurchargePrice { get; set; }
        public double ServicePrice { get; set; }
        public double TotalPrice { get; set; }
        public int NumberOfGardener { get; set; }
    }
}
