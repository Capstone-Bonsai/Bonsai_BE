using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ContractViewModels
{
    public class TaskViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Note { get; set; }
        public DateTime? CompletedTime { get; set; }
    }
}
