using Application.Commons;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface INotificationService
    {
        Task SendMessageForUserId(Guid id, string title, string message);
        Task SendToStaff(string title, string message);
        Task<Pagination<Notification>> GetNotification(Guid userId, int pageIndex, int pageSize);
        Task<Notification> GetNotificationById(Guid userId, Guid id);
    }
}
