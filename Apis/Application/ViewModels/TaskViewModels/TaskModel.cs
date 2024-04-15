using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.TaskViewModels
{
    public class TaskModel
    {
        public Guid ServiceOrderId { get; set; }
        public List<Guid> FinishedTasks { get; set; } = default!;
    }
}
