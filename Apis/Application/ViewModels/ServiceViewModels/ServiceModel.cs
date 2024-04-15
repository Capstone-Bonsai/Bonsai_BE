using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.ViewModels.ServiceViewModels
{
    public class ServiceModel
    {
        public Guid ServiceTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [NotMapped]
        public IList<Guid>? ServiceBaseTaskId { get; set; }
        [NotMapped]
        public IFormFile? Image { get; set; }
    }
}
