using Application.ViewModels.TaskViewModels;
using Application.ViewModels.UserViewModels;
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
    public class OverallServiceOrderViewModel
    {
        public Guid Id { get; set; }
        public Guid ServiceGardenId { get; set; }
        public Guid? CustomerBonsaiId { get; set; }
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
        public ServiceOrderStatus ServiceOrderStatus { get; set; }
        public ServiceType ServiceType { get; set; }
        public int NumberOfGardener { get; set; }
        public IList<BonsaiCareStep> BonsaiCareSteps { get; set; }
        public IList<Contract> Contracts { get; set; }
        public IList<GardenCareTask> GardenCareTasks { get; set; }
        public IList<ServiceOrderGardener> ServiceOrderGardeners { get; set; }
        public IList<ServiceOrderTransaction> ServiceOrderTransactions { get; set; }
        public IList<Complaint> Complaints { get; set; }
        [NotMapped]
        public List<TaskOfServiceOrder> TaskOfServiceOrders { get; set; } = default!;
        [NotMapped]
        public List<UserViewModel> GardenersOfServiceOrder { get; set; } = default!;
    }
}
