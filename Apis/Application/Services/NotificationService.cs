using Application.Commons;
using Application.Hubs;
using Application.Interfaces;
using Application.Utils;
using Domain.Entities;
using Firebase.Auth;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        public NotificationService(IHubContext<NotificationHub> hubContext, IUserConnectionService userConnectionService, IUnitOfWork unitOfWork, IdUtil idUtil, UserManager<ApplicationUser> userManager)
        {
            _hubContext = hubContext;
            _userConnectionService = userConnectionService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task SendMessageForUserId(Guid id, string title, string message)
        {
            var connectionId = _userConnectionService.GetConnectionIdForUser(id.ToString());
            if (connectionId != null)
            {
                await _unitOfWork.NotificationRepository.AddAsync(new Notification()
                {
                    Title = title,
                    UserId = id.ToString(),
                    Message = message,
                });
                await _unitOfWork.SaveChangeAsync();
                await _hubContext.Clients.Client(connectionId).SendAsync(title, message);
            }  
        }
        public async Task SendToStaff(string title, string message)
        {
            var connectionIds = NotificationHub._staffConnections;
            var staff = await _userManager.GetUsersInRoleAsync("Staff");
            List<Notification> notifications = new List<Notification>();
            foreach(ApplicationUser user in staff)
            {
                notifications.Add(new Notification()
                {
                    UserId = user.Id,
                    Title = title,
                    Message = message,
                });
            }
            await _unitOfWork.NotificationRepository.AddRangeAsync(notifications);
            await _unitOfWork.SaveChangeAsync();
            foreach (var connectionId in connectionIds)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync(title, message);
            }
        }
        public async Task<Pagination<Notification>> GetNotification(Guid userId, int pageIndex, int pageSize)
        {
            Pagination<Notification> notifications = new Pagination<Notification>();
            notifications = await _unitOfWork.NotificationRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => x.UserId == userId.ToString().ToLower());
            return notifications;
        }
    }
}
