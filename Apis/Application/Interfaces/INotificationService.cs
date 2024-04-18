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
        Task SendMessageForUserId(Guid id, string message);
        Task SendToStaff(string message);
        Task<Pagination<Notification>> GetNotification(bool isCustomer, Guid customerId, int pageIndex, int pageSize);
    }
}
