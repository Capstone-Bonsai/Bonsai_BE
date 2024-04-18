using Application.Commons;
using Application.Hubs;
using Application.Interfaces;
using Application.Utils;
using Domain.Entities;
using Firebase.Auth;
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
        private readonly IUnitOfWork _unitOfWork;
        public NotificationService(IHubContext<NotificationHub> hubContext, IUserConnectionService userConnectionService, IUnitOfWork unitOfWork, IdUtil idUtil)
        {
            _hubContext = hubContext;
            _userConnectionService = userConnectionService;
            _unitOfWork = unitOfWork;
        }
        public async Task SendMessageForUserId(Guid id, string message)
        {
            var connectionId = _userConnectionService.GetConnectionIdForUser(id.ToString());
            await _unitOfWork.NotificationRepository.AddAsync(new Notification()
            {
                Title = "Tittle",
                Role = "Customer",
                UserId = id,
                Message = message,
            });
            await _unitOfWork.SaveChangeAsync();
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
        public async Task<Pagination<Notification>> GetNotification(bool isCustomer, Guid customerId, int pageIndex, int pageSize)
        {
            Pagination<Notification> notifications = new Pagination<Notification>();
            if (isCustomer)
            {
                notifications = await _unitOfWork.NotificationRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => x.Role.Equals("Customer") && x.UserId == customerId);
            }
            return notifications;
        }
    }
}
