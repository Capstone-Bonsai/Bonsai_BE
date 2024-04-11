using Application.Commons;
using Application.ViewModels.ServiceGardenViewModels;
using Domain.Entities;


namespace Application.Interfaces
{
    public interface IServiceGardenService
    {
        Task<ServiceGarden> AddServiceGarden(ServiceGardenModel serviceGardenModel, Guid userId, bool isCustomer);
        Task CancelServiceGarden(Guid serviceGardenId);
        Task DenyServiceGarden(Guid serviceGardenId);
        Task<Pagination<ServiceGarden>> GetServiceGarden(int pageIndex, int pageSize, bool isCustomer, Guid id);
        Task AcceptServiceGarden(Guid serviceGardenId);
        Task<ServiceGarden> GetServiceGardenById(Guid Id, bool isCustomer, Guid userIds);
    }
}
