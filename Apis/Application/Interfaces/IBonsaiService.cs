using Application.Commons;
using Application.ViewModels.BonsaiViewModel;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBonsaiService
    {
        Task<Pagination<Bonsai>> GetPagination(int pageIndex, int pageSize, bool isAdmin = false);
        Task<Pagination<Bonsai>> GetAll(bool isAdmin = false);
        Task<Pagination<Bonsai>> GetByFilter(int pageIndex, int pageSize, FilterBonsaiModel filterBonsaiModel, bool isAdmin = false);
        Task<Bonsai?> GetById(Guid id, bool isAdmin = false);
        Task AddAsync(BonsaiModel bonsaiModel);
        Task Update(Guid id, BonsaiModel bonsaiModel);
        Task Delete(Guid id);
        Task<Pagination<Bonsai>> GetBoughtBonsai(Guid id);
    }
}
