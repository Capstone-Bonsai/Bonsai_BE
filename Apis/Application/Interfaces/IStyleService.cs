using Application.Commons;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.StyleViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStyleService
    {
        Task<Pagination<Style>> GetStyles();
        Task<Style?> GetStyleById(Guid id);
        Task AddStyle(StyleModel styleModel);
        Task UpdateStyle(Guid id, StyleModel styleModel);
        Task DeleteStyle(Guid id);
    }
}
