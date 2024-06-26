﻿using Application.Commons;
using Application.ViewModels.UserViewModels;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces
{
    public interface IUserService
    {
        public Task<List<string>> ChangePasswordAsync(ChangePassModel model, string userId);
        public Task<IList<string>> UpdateUserAsync(UserRequestModel model, string userId);
        public Task<ApplicationUser> GetUserByIdAsync(string userId);
        public Task<UserViewModel> GetUserById(string userId);

        public Task<Pagination<UserViewModel>> GetListUserAsync(int pageIndex = 0, int pageSize = 20);
        public Task<string> LockOrUnlockUser(string userId);
        public Task<IList<string>> CreateUserAccount(UserCreateModel model);
        public Task<List<string>> GetListRoleAsync();
        public Task Delete(string role, ApplicationUser user);
        Task<Pagination<GardenerViewModel>> GetListGardenerAsync(int pageIndex, int pageSize, Guid contractId);
        Task<Pagination<GardenerViewModel>> GetListGardenerAsync();

    }
}
