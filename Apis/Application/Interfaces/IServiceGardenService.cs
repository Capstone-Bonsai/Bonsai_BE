using Application.Commons;
using Application.ViewModels.ServiceGardenViewModels;
using Domain.Entities;


namespace Application.Interfaces
{
    public interface IServiceGardenService
    {
        Task<ServiceGarden> AddServiceGarden(ServiceGardenModel serviceGardenModel);
        Task<Pagination<ServiceGarden>> GetServiceGardenByGardenId(Guid customerGardenId);
        Task CancelServiceGarden(Guid serviceGardenId);
        Task DenyServiceGarden(Guid serviceGardenId);
    }
}
