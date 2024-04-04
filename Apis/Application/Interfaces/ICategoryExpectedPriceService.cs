using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICategoryExpectedPriceService
    {
        public double GetPrice(float height);
        Task CreateAsync(IFormFile file);
        Task UpdateAsync(IFormFile file);
    }
}
