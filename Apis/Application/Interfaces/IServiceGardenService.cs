using Application.Commons;
using Application.ViewModels.ServiceGardenViewModels;
using Domain.Entities;


namespace Application.Interfaces
{
    public interface IServiceGardenService
    {
        Task<ServiceGarden> AddServiceGarden(ServiceGardenModel serviceGardenModel);
        Task<Pagination<ServiceGarden>> GetServiceGardenByCustomerId(Guid customerId, int pageIndex, int pageSize);
        Task CancelServiceGarden(Guid serviceGardenId);
        Task DenyServiceGarden(Guid serviceGardenId);
        Task<Pagination<ServiceGarden>> GetServiceGarden(int pageIndex, int pageSize);
        Task AcceptServiceGarden(Guid serviceGardenId);
        Task<ServiceGarden> GetServiceGardenById(Guid Id);
    }
}
