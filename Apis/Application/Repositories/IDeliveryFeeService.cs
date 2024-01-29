using Application.ViewModels.DeliveryFeeViewModels;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IDeliveryFeeService
    {
        public Task CreateAsync(IFormFile file);
        public Task UpdateAsync(IFormFile file);
        public Task<DeliveryFeeViewModel> GetAllAsync();
        public Task<FeeViewModel> CalculateFee(string destination, double price);
    }
}
