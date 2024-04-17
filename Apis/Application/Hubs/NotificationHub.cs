using Application.Interfaces;
using Application.ViewModels.MessageViewModels;
using Domain.Entities;
using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public static List<string> _staffConnections = new List<string>();
        private readonly IClaimsService _claimsService;
        private readonly UserManager<ApplicationUser> _userManager;
        public NotificationHub(IClaimsService claimsService, UserManager<ApplicationUser> userManager)
        {
            _claimsService = claimsService;
            _userManager = userManager;
        }
        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            string userId = _claimsService.GetCurrentUserId.ToString();
            var user = await _userManager.FindByIdAsync(userId);
            var isStaff = await _userManager.IsInRoleAsync(user, "Staff");
            if (isStaff)
            {
                if (!_staffConnections.Contains(userId))
                {
                    _staffConnections.Add(connectionId);
                }        
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;
            string userId = _claimsService.GetCurrentUserId.ToString();
            var user = await _userManager.FindByIdAsync(userId);
            var isStaff = await _userManager.IsInRoleAsync(user, "Staff");
            if (isStaff && _staffConnections.Contains(connectionId))
            {
                _staffConnections.Remove(connectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
