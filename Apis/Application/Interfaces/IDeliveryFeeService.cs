using Application.ViewModels.DeliveryFeeViewModels;
using Domain.Entities;
using Domain.Enums;
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
        public Task<DeliveryFeeDisplayModel> GetAllAsync();
        public Task<FeeViewModel> CalculateFee(string destination,IList<Guid> listBonsaiId);
        public Task<FeeViewModel> CalculateFeeOrder(string destination, IList<Guid> listBonsaiId);
        public Task<DistanceResponse> GetDistanse(string destination);
        public Task<double> CalculateFeeOfBonsai(DeliverySize size, int distance);
        public Task<double> TestCalcutale(/*Guid bonsaiId,*/ int distance);
       

    }
}
