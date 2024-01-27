using Application.ViewModels.ProductTagViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProductTagService
    {
        Task AddAsync(ProductTagModel productTagModel);
        Task SoftDeleteAsync(ProductTagModel productTagModel);
    }
}
