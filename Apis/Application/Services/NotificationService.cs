using Application.Hubs;
using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUserConnectionService _userConnectionService;
        public NotificationService(IHubContext<NotificationHub> hubContext, IUserConnectionService userConnectionService)
        {
            _hubContext = hubContext;
            _userConnectionService = userConnectionService;
        }
        public async Task SendMessageForUserId(Guid id, string message)
        {
            var connectionId = _userConnectionService.GetConnectionIdForUser(id.ToString());
            await _hubContext.Clients.User(connectionId).SendAsync("Notification", message);
        }
        public async Task SendToStaff(string message)
        {
            var connectionIds = NotificationHub._staffConnections;
            foreach (var connectionId in connectionIds)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("Notification", message);
            }
        }
    }
}
