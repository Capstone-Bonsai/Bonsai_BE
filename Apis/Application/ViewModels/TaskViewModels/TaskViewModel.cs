using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.TaskViewModels
{
    public class TaskViewModel
    {
        public Guid ContractId { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public List<TaskOfServiceOrder> TaskOfServiceOrders { get; set; } = default!;
    }
    public class TaskOfServiceOrder
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Note { get; set; }
        public DateTime? CompletedTime { get; set; }
    }
}
