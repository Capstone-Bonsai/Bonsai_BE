using Application.Commons;
using Application.ViewModels.ServiceViewModels;

namespace Application.Interfaces
{
    public interface IServiceService
    {
        public Task<Pagination<ServiceViewModel>> GetServicePagination(int pageIndex = 0, int pageSize = 10);
        public Task<ServiceViewModel> GetServiceById(Guid id, string? userId);
        public Task<IList<string>> AddService(ServiceModel model);
        public Task<IList<string>> UpdateService(Guid id, ServiceModel model);
        public Task DeleteService(Guid id);
        Task<Pagination<ServiceViewModel>> GetServicePagination(int pageIndex, int pageSize, Guid serviceTypeId, Guid? customerBonsaId);
    }
}
