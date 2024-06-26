﻿using Application.ViewModels.TaskViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskViewModel> GetTasksOfServiceOrder(Guid serviceOrderId);
        Task UpdateProgress(TaskModel taskModel);
        Task ClearProgress(Guid serviceOrderId);
        Task UpdateNote(UpdateNoteModel updateNoteModel);
    }
}
