using Application.Interfaces;
using Application.Services;
using Application.ViewModels.MessageViewModels;
using Domain.Entities;
using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Hubs
{
    //[Authorize]
    public class NotificationHub : Hub
    {
        public static List<string> _staffConnections = new List<string>();
        private readonly IClaimsService _claimsService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserConnectionService _userConnectionService;
        private readonly IConfiguration _configuration;

        public NotificationHub(IClaimsService claimsService, UserManager<ApplicationUser> userManager, IUserConnectionService userConnectionService, IConfiguration configuration)
        {
            _claimsService = claimsService;
            _userManager = userManager;
            _userConnectionService = userConnectionService;
            _configuration = configuration;
        }
        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            var httpContext = Context.GetHttpContext();
            var token = httpContext.Request.Query["access_token"];
            if (token.IsNullOrEmpty())
            { 
                Context.Abort();
                return; }
            string userId = JwtHandler(token);
            if (userId == null)
            {
                Context.Abort();
                return;
            }
            var user = await _userManager.FindByIdAsync(userId);
            var isStaff = await _userManager.IsInRoleAsync(user, "Staff");
            if (isStaff)
            {
                if (!_staffConnections.Contains(user.Id))
                {
                    _staffConnections.Add(connectionId);
                }
            }
            else
            {
                _userConnectionService.AddOrUpdateConnectionId(user.Id, connectionId);
            }
            int numberOfConnectedClients = _staffConnections.Count + _userConnectionService.GetAllConnectionIds().Count();
            await Clients.Client(connectionId).SendAsync("Hello", numberOfConnectedClients);
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;
            var httpContext = Context.GetHttpContext();
            var token = httpContext.Request.Query["access_token"];
            if (token.IsNullOrEmpty())
            {
                Context.Abort();
                return;
            }
            string userId = JwtHandler(token);
            if (userId == null)
            {
                Context.Abort();
                return;
            }
            var user = await _userManager.FindByIdAsync(userId);
            var isStaff = await _userManager.IsInRoleAsync(user, "Staff");
            if (isStaff && _staffConnections.Contains(connectionId))
            {
                _staffConnections.Remove(connectionId);
            }
            else
            {
                _userConnectionService.RemoveConnectionId(user.Id);
            }

            await base.OnDisconnectedAsync(exception);
        }
        private string JwtHandler(string token)
        {
            string secretKey = _configuration["JWT:SecrectKey"];

            // Giải mã JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidAudience = _configuration["JWT:ValidAudience"],
                ValidIssuer = _configuration["JWT:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecrectKey"]))
            };

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return claimsPrincipal.FindFirstValue("userId").ToString(); ;

            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return null;
            }
        }
    }
}
