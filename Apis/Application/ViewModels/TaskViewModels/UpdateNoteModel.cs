using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.TaskViewModels
{
    public class UpdateNoteModel
    {
        public Guid ServiceOrderId { get; set; }
        public List<TaskNote> TaskNotes { get; set; } = default!;    
    }
    public class TaskNote
    {
        public Guid TaskId { get; set; }
        public String Note { get; set; } = default!;
    }
}
