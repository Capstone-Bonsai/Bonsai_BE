using Application.ViewModels.UserViewModels;

namespace Application.Interfaces
{
    public interface IUserService
    {
        public Task<List<string>> ChangePasswordAsync(ChangePassModel model, string userId);

        public Task<List<string>> UpdateUserAsync(ChangePassModel model, string userId);
    }
}
