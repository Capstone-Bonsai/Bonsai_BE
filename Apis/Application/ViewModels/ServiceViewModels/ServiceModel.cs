using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
namespace Application.ViewModels.ServiceViewModels
{
    public class ServiceModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double StandardPrice { get; set; }
        public bool IsDisable { get; set; }

        public IFormFile? Image { get; set; }
        public ServiceType ServiceType { get; set; }
        public IList<Guid>? ServiceBaseTaskId { get; set; }
    }
}
