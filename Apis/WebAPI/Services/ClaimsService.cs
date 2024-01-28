using Application.Interfaces;
using System.Security.Claims;

namespace WebAPI.Services
{
    public class ClaimsService : IClaimsService
    {
        public ClaimsService(IHttpContextAccessor httpContextAccessor)
        {
            // todo implementation to get the current userId
            var Id = httpContextAccessor.HttpContext?.User?.FindFirstValue("userId");
            var isAdmin = httpContextAccessor.HttpContext?.User?.FindFirstValue("isAdmin");

            GetCurrentUserId = string.IsNullOrEmpty(Id) ? Guid.Empty : Guid.Parse(Id);
            if (string.IsNullOrEmpty(isAdmin))
                GetIsAdmin = false;
            else if(isAdmin.Equals("True")) GetIsAdmin = true;
            else GetIsAdmin = false;
        }

        public Guid GetCurrentUserId { get; }
        public bool GetIsAdmin { get; }
    }
}
