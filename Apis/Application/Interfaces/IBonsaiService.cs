using Application.Commons;
using Application.ViewModels.BonsaiViewModel;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBonsaiService
    {
        public Task<Pagination<Bonsai>> GetPagination(int pageIndex, int pageSize, bool isAdmin = false);
        public Task<Pagination<Bonsai>> GetAll(bool isAdmin = false);
        public Task<Pagination<Bonsai>> GetByFilter(int pageIndex, int pageSize, FilterBonsaiModel filterBonsaiModel, bool isAdmin = false);
        public Task<Bonsai?> GetById(Guid id, bool isAdmin = false);
        public Task AddAsync(BonsaiModel bonsaiModel);
        public Task Update(Guid id, BonsaiModel bonsaiModel);
        public Task Delete(Guid id);
        public Task<Pagination<Bonsai>> GetBoughtBonsai(Guid id);
        public Task<Pagination<Bonsai>> GetByCategory(int pageIndex, int pageSize, Guid categoryId);
        public Task DisableBonsai(Guid id);
        public Task<List<Bonsai>> getCurrentCart(List<Guid> bonsaiId);
    }
}
