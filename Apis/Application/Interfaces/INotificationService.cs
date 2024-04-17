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
    }
}
