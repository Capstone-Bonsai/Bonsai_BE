using Application.ViewModels.UserViewModels;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserService
    {
        public Task<List<string>> ChangePasswordAsync(ChangePassModel model, string userId);
        public Task<IList<string>> UpdateUserAsync(UserRequestModel model, string userId);
        public Task<ApplicationUser> GetUserByIdAsync(string userId);
    }
}
